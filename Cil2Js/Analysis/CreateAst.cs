using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public class CreateAst {

        class StackSizeVisitor : AstRecursiveVisitor {

            protected override ICode VisitCil(StmtCil s) {
                var endStackSize = this.StackSizeAnalysis(s.Insts, s.StartStackSize);
                s.EndStackSize = endStackSize;
                var conts = VisitorFindContinuations.Get(s.EndCil);
                Action<Stmt, int> setStackSize = null;
                setStackSize = (stmt, stackSize) => {
                    // Set all try and catch stacksizes recursively, in case multiple trys start on the same instruction
                    switch (stmt.StmtType) {
                    case Stmt.NodeType.Cil:
                        ((StmtCil)stmt).StartStackSize = stackSize;
                        return;
                    case Stmt.NodeType.Try:
                        var stmtTry = (StmtTry)stmt;
                        setStackSize(stmtTry.Try, stackSize);
                        if (stmtTry.Catch != null) {
                            setStackSize(stmtTry.Catch, 1);
                        }
                        return;
                    default:
                        throw new NotSupportedException("Should not be seeing: " + stmt.StmtType);
                    }
                };
                foreach (var cont in conts) {
                    setStackSize(cont.To, endStackSize);
                }
                return base.VisitCil(s);
            }

            private int StackSizeAnalysis(IEnumerable<Instruction> insts, int startStackSize) {
                int stackSize = startStackSize;
                foreach (var inst in insts) {
                    switch (inst.OpCode.StackBehaviourPop) {
                    case StackBehaviour.Pop0: break;
                    case StackBehaviour.Popi:
                    case StackBehaviour.Popref:
                    case StackBehaviour.Pop1: stackSize--; break;
                    case StackBehaviour.Popi_pop1:
                    case StackBehaviour.Popi_popi:
                    case StackBehaviour.Popi_popi8:
                    case StackBehaviour.Popi_popr4:
                    case StackBehaviour.Popi_popr8:
                    case StackBehaviour.Popref_pop1:
                    case StackBehaviour.Popref_popi:
                    case StackBehaviour.Pop1_pop1: stackSize -= 2; break;
                    case StackBehaviour.Popref_popi_popi:
                    case StackBehaviour.Popref_popi_popi8:
                    case StackBehaviour.Popref_popi_popr4:
                    case StackBehaviour.Popref_popi_popr8:
                    case StackBehaviour.Popref_popi_popref:
                    case StackBehaviour.Popi_popi_popi: stackSize -= 3; break;
                    case StackBehaviour.PopAll: stackSize = 0; break;
                    case StackBehaviour.Varpop:
                        switch (inst.OpCode.Code) {
                        case Code.Ret: stackSize = 0; break;
                        case Code.Newobj:
                        case Code.Callvirt:
                        case Code.Call: stackSize -= ((MethodReference)inst.Operand).Parameters.Count; break;
                        default: throw new NotImplementedException("Cannot handle: " + inst.OpCode);
                        }
                        break;
                    default: throw new NotImplementedException("Cannot handle: " + inst.OpCode.StackBehaviourPop);
                    }
                    switch (inst.OpCode.StackBehaviourPush) {
                    case StackBehaviour.Push0: break;
                    case StackBehaviour.Pushi8:
                    case StackBehaviour.Pushr4:
                    case StackBehaviour.Pushr8:
                    case StackBehaviour.Pushref:
                    case StackBehaviour.Pushi:
                    case StackBehaviour.Push1: stackSize++; break;
                    case StackBehaviour.Push1_push1: stackSize += 2; break;
                    case StackBehaviour.Varpush:
                        switch (inst.OpCode.Code) {
                        case Code.Newobj: stackSize++; break;
                        case Code.Callvirt:
                        case Code.Call: stackSize += ((MethodReference)inst.Operand).ReturnType.IsVoid() ? 0 : 1; break;
                        default: throw new NotImplementedException("Cannot handle: " + inst.OpCode);
                        }
                        break;
                    default: throw new NotImplementedException("Cannot handle: " + inst.OpCode.StackBehaviourPush);
                    }
                }
                return stackSize;
            }

        }

        public CreateAst(MethodDefinition method) {
            this.method = method;
        }

        private MethodDefinition method;
        private TypeReference boolean;
        private IEnumerable<Instruction> methodBlockStarts;
        private Dictionary<Instruction, List<Stmt>> blockMap = new Dictionary<Instruction, List<Stmt>>();
        private List<IInstructionMappable> mappable = new List<IInstructionMappable>();

        public Stmt Create() {
            if (!this.method.HasBody) {
                throw new ArgumentException("Method has no body, cannot create AST");
            }
            this.boolean = this.method.Module.TypeSystem.Boolean;
            var body = this.method.Body;
            // Pre-calculate all method block starts
            this.methodBlockStarts = body.Instructions
                .SelectMany(x => {
                    switch (x.OpCode.FlowControl) {
                    case FlowControl.Cond_Branch:
                    case FlowControl.Branch:
                        return new[] { (Instruction)x.Operand, x.Next };
                    case FlowControl.Throw:
                    case FlowControl.Return:
                        return new[] { x.Next };
                    default:
                        return Enumerable.Empty<Instruction>();
                    }
                })
                .Concat(body.ExceptionHandlers.SelectMany(x => new[] {
                    x.TryStart, x.TryEnd, x.HandlerStart, x.HandlerEnd
                }))
                .Concat(body.Instructions.First())
                .Where(x => x != null)
                .Distinct()
                .OrderBy(x => x.Offset)
                .ToArray();
            // Create all method blocks
            this.CreatePart(body.Instructions, body.ExceptionHandlers);
            // Map continuations and try statements
            // Must continue to outermost Try statement, if one exists, rather than directly to the first CIL block
            foreach (var mappable in this.mappable) {
                mappable.Map(this.blockMap);
            }
            var stmt0 = this.blockMap[body.Instructions.First()].First();
            var vStackSize = new StackSizeVisitor();
            vStackSize.Visit(stmt0);
            return stmt0;
        }

        private void CreatePart(IEnumerable<Instruction> insts, IEnumerable<ExceptionHandler> exs) {
            var stmtStart = insts.First();
            var end = insts.Last();
            var inst = insts.First();
            for (; ; ) {
                // Get the outermost 'try' statement starting on this instruction, if there is one
                var ex = exs.Where(x => x.TryStart == inst).OrderByDescending(x => x.HandlerEnd.Offset).FirstOrDefault();
                if (ex != null) {
                    if (inst != stmtStart) {
                        // Build preceding code block up until this 'try' statement
                        this.BuildBlock(stmtStart.GetRange(inst.Previous));
                    }
                    // Build this 'try' statement
                    var tryExs = exs.Where(x => x.TryStart.Offset >= ex.TryStart.Offset && x.TryEnd.Offset < ex.TryEnd.Offset).ToArray();
                    this.CreatePart(ex.TryStart.GetRange(ex.TryEnd.Previous), tryExs);
                    // Build the 'catch' or 'finally' handler statement
                    var handlerExs = exs.Where(x => x.TryStart.Offset >= ex.HandlerStart.Offset && x.TryEnd.Offset < ex.HandlerEnd.Offset).ToArray();
                    this.CreatePart(ex.HandlerStart.GetRange(ex.HandlerEnd.Previous), handlerExs);
                    StmtTry tryStmt;
                    if (ex.HandlerType == ExceptionHandlerType.Catch) {
                        tryStmt = new StmtTry(ex.TryStart, ex.HandlerStart, null, ex.CatchType);
                    } else {
                        tryStmt = new StmtTry(ex.TryStart, null, ex.HandlerStart, null);
                    }
                    this.mappable.Add(tryStmt);
                    // Put all 'try' statements in outer-first order. CIL will be at the end of the list
                    this.blockMap[inst].Insert(0, tryStmt);
                    stmtStart = ex.HandlerEnd;
                    inst = stmtStart;
                }
                if (inst == null || inst == end) {
                    break;
                }
                inst = inst.Next;
            }
            if (stmtStart != null) {
                // Build the final statement
                this.BuildBlock(stmtStart.GetRange(end));
            }
        }

        private void BuildBlock(IEnumerable<Instruction> insts) {
            var inst0 = insts.First();
            var instN = insts.Last();
            // Any instruction in the method could target this block
            var blockStarts = this.methodBlockStarts
                .Where(x => x.Offset >= inst0.Offset && x.Offset <= instN.Offset)
                .ToArray();
            // For each block create a StmtCil, with the correct ending.
            // Endings that require a expression have them set to null; they will be filled in during CIL decoding
            for (int i = 0; i < blockStarts.Length; i++) {
                var start = blockStarts[i];
                var end = i == blockStarts.Length - 1 ? insts.Last() : blockStarts[i + 1].Previous;
                var blockInsts = start.GetRange(end);
                Stmt blockEndStmt;
                var code = end.OpCode.Code;
                switch (end.OpCode.FlowControl) {
                case FlowControl.Cond_Branch:
                    var ifTrue = new StmtContinuation((Instruction)end.Operand, false);
                    var ifFalse = new StmtContinuation(end.Next, false);
                    this.mappable.Add(ifTrue);
                    this.mappable.Add(ifFalse);
                    blockEndStmt = new StmtIf(new ExprVarInstResult(end, this.boolean), ifTrue, ifFalse);
                    break;
                case FlowControl.Branch:
                    var leaveProtectedRegion = code == Code.Leave || code == Code.Leave_S;
                    blockEndStmt = new StmtContinuation((Instruction)end.Operand, leaveProtectedRegion);
                    this.mappable.Add((StmtContinuation)blockEndStmt);
                    break;
                case FlowControl.Next:
                case FlowControl.Call:
                    blockEndStmt = new StmtContinuation(end.Next, false);
                    this.mappable.Add((StmtContinuation)blockEndStmt);
                    break;
                case FlowControl.Return:
                    blockEndStmt = null; // 'Return' created when converting CIL
                    //if (code == Code.Endfinally || code == Code.Endfilter) {
                    //    blockEndStmt = null;
                    //} else {
                    //    blockEndStmt = new StmtReturn(null);
                    //}
                    break;
                case FlowControl.Throw:
                    blockEndStmt = null; // Throw created when converting CIL
                    break;
                default:
                    throw new NotImplementedException("Cannot handle: " + end.OpCode.FlowControl);
                }
                var block = new StmtCil(this.method, blockInsts, blockEndStmt);
                this.blockMap.Add(start, new List<Stmt> { block });
            }
        }

    }
}
