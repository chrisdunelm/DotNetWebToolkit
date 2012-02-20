using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public partial class VisitorConvertCilToSsa : AstRecursiveVisitor {

        class VisitorFindInstResults : AstRecursiveVisitor {

            public List<ExprVarInstResult> instResults = new List<ExprVarInstResult>();

            protected override ICode VisitVarInstResult(ExprVarInstResult e) {
                this.instResults.Add(e);
                return base.VisitVarInstResult(e);
            }

        }

        class VisitorCounter : AstRecursiveVisitor {

            public static int Count(ICode ast, ICode toCount) {
                var v = new VisitorCounter(toCount);
                v.Visit(ast);
                return v.count;
            }

            private VisitorCounter(ICode toCount) {
                this.toCount = toCount;
            }

            private ICode toCount;
            private int count = 0;

            public override ICode Visit(ICode c) {
                if (c == this.toCount) {
                    count++;
                }
                return base.Visit(c);
            }

        }

        // Only have if, switch, cil, continuation, try - no other type of statement
        // Blocks will only start with cil or try
        // Will end with null, if, switch or continuation
        // If ending in 'if', 'then' and 'else' will both have continuations only
        // 'try' statements will have only 0 or 1 catch clauses

        public static ICode V(ICode ast) {
            var v = new VisitorConvertCilToSsa(ast);
            ast = v.Visit(ast);
            foreach (var instResult in v.instResults.Values) {
                var var = new ExprVarLocal(ast.Ctx, instResult.Type);
                ast = VisitorReplace.V(ast, instResult, var);
            }
            return ast;
        }

        private VisitorConvertCilToSsa(ICode root) {
            this.ctx = root.Ctx;
            var v = new VisitorFindInstResults();
            v.Visit(root);
            this.instResults = v.instResults.ToDictionary(x => x.Inst);
            this.CreateOrMergeBsi((Stmt)root, new ExprVarPhi[0],
                this.ctx.MDef.Body.Variables.Select(x => (Expr)new ExprVarLocal(this.ctx, x.VariableType.FullResolve(this.ctx))).ToArray(),
                this.ctx.MRef.Parameters.Select(x => (Expr)new ExprVarParameter(this.ctx, x)).ToArray());
        }

        class BlockInitInfo {
            public ExprVarPhi[] Stack;
            public ExprVarPhi[] Locals;
            public ExprVarPhi[] Args;
        }

        class StmtVarChanged {
            public bool[] Stack;
            public bool[] Locals;
            public bool[] Args;
        }

        private Ctx ctx;
        private Dictionary<Instruction, ExprVarInstResult> instResults;
        private Dictionary<ICode, BlockInitInfo> blockStartInfos = new Dictionary<ICode, BlockInitInfo>();
        private Dictionary<ICode, StmtVarChanged> stmtVarsChanged = new Dictionary<ICode, StmtVarChanged>();

        private void CreateOrMergeBsi(Stmt s, Expr[] stack, Expr[] locals, Expr[] args) {
            if (s.StmtType == Stmt.NodeType.Try) {
                var sTry = (StmtTry)s;
                // It is fine to use 'locals' and 'args' in catch/finally because the phi clustering performed later
                // will conglomerate all the necessary variables
                if (sTry.Catches != null) {
                    var catch0 = sTry.Catches.First();
                    this.CreateOrMergeBsi(catch0.Stmt, new Expr[] { catch0.ExceptionVar }, locals, args);
                }
                if (sTry.Finally != null) {
                    this.CreateOrMergeBsi(sTry.Finally, new Expr[0], locals, args);
                }
                this.CreateOrMergeBsi(sTry.Try, stack, locals, args);
                return;
            }
            if (s.StmtType != Stmt.NodeType.Cil) {
                throw new InvalidCastException("Should not be seeing: " + s.StmtType);
            }
            // Perform create/merge
            Func<Expr, IEnumerable<Expr>> flattenPhiExprs = null;
            flattenPhiExprs = e => {
                if (e.ExprType == Expr.NodeType.VarPhi) {
                    return ((ExprVarPhi)e).Exprs.SelectMany(x => flattenPhiExprs(x));
                }
                return new[] { e };
            };
            Action<ExprVarPhi[], IEnumerable<Expr>> merge = (bsiVars, thisVars) => {
                foreach (var v in bsiVars.Zip(thisVars, (a, b) => new { phi = a, add = b })) {
                    if (v.add != null) {
                        v.phi.Exprs = flattenPhiExprs(v.add).Concat(v.phi.Exprs).Where(x => x != null && x != v.phi).Distinct().ToArray();
                    }
                }
            };
            BlockInitInfo bsi;
            if (!this.blockStartInfos.TryGetValue(s, out bsi)) {
                Func<IEnumerable<Expr>, ExprVarPhi[]> create = exprs => exprs.Select(x => {
                    if (x == null) {
                        return new ExprVarPhi(this.ctx) { Exprs = new Expr[0] };
                    }
                    if (x.ExprType == Expr.NodeType.VarPhi) {
                        return (ExprVarPhi)x;
                    }
                    return new ExprVarPhi(this.ctx) { Exprs = new[] { x } };
                }).ToArray();
                bsi = new BlockInitInfo {
                    Stack = create(stack),
                    Locals = create(locals),
                    Args = create(args),
                };
                this.blockStartInfos.Add(s, bsi);
            } else {
                merge(bsi.Stack, stack);
                merge(bsi.Locals, locals);
                merge(bsi.Args, args);
                // Forward-merge through already-processed nodes for vars that are not changed in a node
                var fmSeen = new HashSet<Stmt>();
                Action<Stmt> forwardMerge = null;
                forwardMerge = (stmt) => {
                    if (fmSeen.Add(stmt)) {
                        var fmBsi = this.blockStartInfos.ValueOrDefault(stmt);
                        var fmChanges = this.stmtVarsChanged.ValueOrDefault(stmt);
                        if (fmBsi != null && fmChanges != null) {
                            var fmStack = fmBsi.Stack.Take(fmChanges.Stack.Length).Select((x, i) => fmChanges.Stack[i] ? x : null).ToArray();
                            var fmLocals = fmBsi.Locals.Take(fmChanges.Locals.Length).Select((x, i) => fmChanges.Locals[i] ? x : null).ToArray();
                            var fmArgs = fmBsi.Args.Take(fmChanges.Args.Length).Select((x, i) => fmChanges.Args[i] ? x : null).ToArray();
                            if (fmStack.Any(x => x != null) || fmLocals.Any(x => x != null) || fmArgs.Any(x => x != null)) {
                                var fmConts = VisitorFindContinuations.Get(stmt);
                                foreach (var fmCont in fmConts) {
                                    var fmContBsi = this.blockStartInfos.ValueOrDefault(fmCont.To);
                                    if (fmContBsi != null) {
                                        merge(fmContBsi.Stack, fmStack);
                                        merge(fmContBsi.Locals, fmLocals);
                                        merge(fmContBsi.Args, fmArgs);
                                        forwardMerge(fmCont.To);
                                    }
                                }
                            }
                        }
                    }
                };
                forwardMerge(s);
            }
        }

        protected override ICode VisitCil(StmtCil s) {
            var bsi = this.blockStartInfos[s];
            var stack = new Stack<Expr>(bsi.Stack.Reverse());
            var locals = bsi.Locals.Cast<Expr>().ToArray();
            var args = bsi.Args.Cast<Expr>().ToArray();
            var orgStack = stack.ToArray();
            var orgLocals = locals.ToArray();
            var orgArgs = args.ToArray();
            var cil = new CilProcessor(this.ctx, stack, locals, args, this.instResults);
            var stmts = new List<Stmt>();
            switch (s.BlockType) {
            case StmtCil.SpecialBlock.Normal:
                foreach (var inst in s.Insts) {
                    var stmt = cil.Process(inst);
                    if (stmt != null) {
                        stmts.Add(stmt);
                    }
                }
                break;
            case StmtCil.SpecialBlock.Start:
                // Do nothing
                break;
            case StmtCil.SpecialBlock.End:
                stmts.Add(cil.ProcessReturn());
                break;
            default:
                throw new InvalidOperationException("Invalid block type: " + s.BlockType);
            }
            this.stmtVarsChanged.Add(s, new StmtVarChanged {
                Stack = stack.Zip(orgStack, (a, b) => a == b).ToArray(),
                Locals = locals.Zip(orgLocals, (a, b) => a == b).ToArray(),
                Args = args.Zip(orgArgs, (a, b) => a == b).ToArray(),
            });
            // Merge phi's
            var continuations = VisitorFindContinuations.Get(s);
            foreach (var continuation in continuations) {
                this.CreateOrMergeBsi(continuation.To, stack.ToArray(), locals, args);
            }
            // End
            var next = (Stmt)this.Visit(s.EndCil);
            stmts.Add(next);
            return new StmtBlock(this.ctx, stmts);
        }

    }
}
