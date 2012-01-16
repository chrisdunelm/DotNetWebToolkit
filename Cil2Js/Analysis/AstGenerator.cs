using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public class AstGenerator {

        class StackSizeVisitor : AstRecursiveVisitor {

            protected override ICode VisitCil(StmtCil s) {
                var endStackSize = this.StackSizeAnalysis(s.Insts, s.StartStackSize);
                s.EndStackSize = endStackSize;
                var conts = VisitorFindContinuations.Get(s.EndCil);
                Action<Stmt, int> setStackSize = null;
                setStackSize = (stmt, stackSize) => {
                    // Set all try and catch stacksizes recursively, to handle multiple trys start on the same instruction
                    switch (stmt.StmtType) {
                    case Stmt.NodeType.Cil:
                        ((StmtCil)stmt).StartStackSize = stackSize;
                        return;
                    case Stmt.NodeType.Try:
                        var stmtTry = (StmtTry)stmt;
                        setStackSize(stmtTry.Try, stackSize);
                        if (stmtTry.Catches != null) {
                            setStackSize(stmtTry.Catches.First().Stmt, 1);
                        }
                        // 'Finally' stack sizes do not need setting, as they will have defaulted to 0
                        // and this will always be correct
                        return;
                    case Stmt.NodeType.Return:
                        // do nothing
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
                if (insts == null) {
                    return startStackSize;
                }
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

        public static Stmt CreateBlockedCilAst(Ctx ctx) {
            var gen = new AstGenerator(ctx);
            return gen.Create();
        }

        private AstGenerator(Ctx ctx) {
            this.ctx = ctx;
            this.endBlock = new StmtCil(this.ctx, null, null, StmtCil.SpecialBlock.End);
        }

        private Ctx ctx;
        private Stmt endBlock;
        private IEnumerable<Instruction> methodBlockStarts;
        private Dictionary<Instruction, List<Stmt>> blockMap = new Dictionary<Instruction, List<Stmt>>();
        private List<IInstructionMappable> mappable = new List<IInstructionMappable>();

        public Stmt Create() {
            var mDef = this.ctx.MRef.Resolve();
            if (!mDef.HasBody) {
                throw new ArgumentException("Method has no body, cannot create AST");
            }
            var body = mDef.Body;
            // Pre-calculate all method block starts
            this.methodBlockStarts = body.Instructions
                .SelectMany(x => {
                    switch (x.OpCode.FlowControl) {
                    case FlowControl.Cond_Branch:
                    case FlowControl.Branch:
                        if (x.OpCode.Code == Code.Switch) {
                            return ((Instruction[])x.Operand).Concat(x.Next);
                        } else {
                            return new[] { (Instruction)x.Operand, x.Next };
                        }
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
            // Create entry block, and return it
            var entryBlock = new StmtCil(this.ctx, null, new StmtContinuation(this.ctx, stmt0, false), StmtCil.SpecialBlock.Start);
            return entryBlock;
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
                        this.BuildBlock(stmtStart.GetRange(inst.Previous), end.Next);
                    }
                    // Build this 'try' statement
                    var tryExs = exs.Where(x => x.TryStart.Offset >= ex.TryStart.Offset && x.TryEnd.Offset < ex.TryEnd.Offset).ToArray();
                    this.CreatePart(ex.TryStart.GetRange(ex.TryEnd.Previous), tryExs);
                    // Build the 'catch' or 'finally' handler statement
                    var handlerExs = exs.Where(x => x.TryStart.Offset >= ex.HandlerStart.Offset && x.TryEnd.Offset < ex.HandlerEnd.Offset).ToArray();
                    this.CreatePart(ex.HandlerStart.GetRange(ex.HandlerEnd.Previous), handlerExs);
                    StmtTry tryStmt;
                    if (ex.HandlerType == ExceptionHandlerType.Catch) {
                        tryStmt = new StmtTry(this.ctx, ex.TryStart, ex.HandlerStart, null, ex.CatchType);
                    } else if (ex.HandlerType == ExceptionHandlerType.Finally) {
                        tryStmt = new StmtTry(this.ctx, ex.TryStart, null, ex.HandlerStart, null);
                    } else {
                        throw new NotImplementedException("Cannot handle handler-type: " + ex.HandlerType);
                    }
                    this.mappable.Add(tryStmt);
                    // Put all 'try' statements in outer-first order. CIL will be at the end of the list
                    this.blockMap[inst].Insert(0, tryStmt);
                    stmtStart = ex.HandlerEnd;
                    inst = ex.HandlerEnd.Previous;
                }
                if (inst == null || inst == end) {
                    break;
                }
                inst = inst.Next;
            }
            if (stmtStart.Offset <= end.Offset) {
                // Build the final statement
                this.BuildBlock(stmtStart.GetRange(end), end.Next);
            }
        }

        private void BuildBlock(IEnumerable<Instruction> insts, Instruction startOfNextPart) {
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
                    if (code == Code.Switch) {
                        var cases = ((Instruction[])end.Operand).Select(x => new StmtContinuation(this.ctx, x, false)).ToArray();
                        foreach (var @case in cases) {
                            this.mappable.Add(@case);
                        }
                        var @default = new StmtContinuation(this.ctx, end.Next, false);
                        this.mappable.Add(@default);
                        blockEndStmt = new StmtSwitch(this.ctx, new ExprVarInstResult(this.ctx, end, this.ctx.Int32),
                            cases.Select((x, value) => new StmtSwitch.Case(value, x)).ToArray(),
                            @default);
                    } else {
                        var ifTrue = new StmtContinuation(this.ctx, (Instruction)end.Operand, false);
                        var ifFalse = new StmtContinuation(this.ctx, end.Next, false);
                        this.mappable.Add(ifTrue);
                        this.mappable.Add(ifFalse);
                        blockEndStmt = new StmtIf(this.ctx, new ExprVarInstResult(this.ctx, end, this.ctx.Boolean), ifTrue, ifFalse);
                    }
                    break;
                case FlowControl.Branch:
                    var leaveProtectedRegion = code == Code.Leave || code == Code.Leave_S;
                    blockEndStmt = new StmtContinuation(this.ctx, (Instruction)end.Operand, leaveProtectedRegion);
                    this.mappable.Add((StmtContinuation)blockEndStmt);
                    break;
                case FlowControl.Next:
                case FlowControl.Call:
                    blockEndStmt = new StmtContinuation(this.ctx, end.Next, false);
                    this.mappable.Add((StmtContinuation)blockEndStmt);
                    break;
                case FlowControl.Return:
                    switch (code) {
                    case Code.Endfinally:
                        blockEndStmt = new StmtContinuation(this.ctx, startOfNextPart, true);
                        this.mappable.Add((StmtContinuation)blockEndStmt);
                        break;
                    case Code.Ret:
                        blockEndStmt = new StmtContinuation(this.ctx, this.endBlock, false);
                        blockInsts = start == end ? Enumerable.Empty<Instruction>() : start.GetRange(end.Previous); // Remove 'ret' from statements
                        break;
                    default:
                        blockEndStmt = null;
                        break;
                    }
                    break;
                case FlowControl.Throw:
                    blockEndStmt = null;
                    break;
                default:
                    throw new NotImplementedException("Cannot handle: " + end.OpCode.FlowControl);
                }
                var block = new StmtCil(this.ctx, blockInsts, blockEndStmt);
                this.blockMap.Add(start, new List<Stmt> { block });
            }
        }

    }
}
