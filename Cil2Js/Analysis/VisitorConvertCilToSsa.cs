using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public partial class VisitorConvertCilToSsa : AstRecursiveVisitor {

        class VisitorFindInstResults : AstRecursiveVisitor {

            public List<ExprVarInstResult> instResults = new List<ExprVarInstResult>();

            protected override ICode VisitVarInstResult(ExprVarInstResult e) {
                this.instResults.Add(e);
                return base.VisitVarInstResult(e);
            }

        }

        class VisitorCounter : AstRecursiveVisitor {

            public static int Count(ICode c, ICode toCount) {
                var v = new VisitorCounter(toCount);
                v.Visit(c);
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

        // Only have if, cil, continuation, return, try, throw - no other type of statement

        public static ICode V(MethodDefinition method, ICode c) {
            var v = new VisitorConvertCilToSsa(method, c);
            c = v.Visit(c);
            // Convert all InstResults to normal Vars
            var vFindInstResults = new VisitorFindInstResults();
            vFindInstResults.Visit(c);
            foreach (var instResult in vFindInstResults.instResults) {
                var var = new ExprVarLocal(instResult.Type);
                c = VisitorReplace.V(c, instResult, var);
            }
            return c;
        }

        private VisitorConvertCilToSsa(MethodDefinition method, ICode root) {
            this.method = method;
            var v = new VisitorFindInstResults();
            v.Visit(root);
            this.instResults = v.instResults.ToDictionary(x => x.Inst);
            this.blockStartInfos.Add(root, new BlockInitInfo {
                Stack = new ExprVarPhi[0],
                Locals = Enumerable.Range(0, method.Body.Variables.Count).Select(x => new ExprVarPhi(method) { Exprs = Enumerable.Empty<Expr>() }).ToArray(),
                Args = method.Parameters.Select(x => new ExprVarPhi(method) { Exprs = new[] { new ExprVarParameter(x) } }).ToArray(),
            });
        }

        class BlockInitInfo {
            public ExprVarPhi[] Stack;
            public ExprVarPhi[] Locals;
            public ExprVarPhi[] Args;
        }

        private MethodDefinition method;
        private Dictionary<Instruction, ExprVarInstResult> instResults;
        private Dictionary<ICode, BlockInitInfo> blockStartInfos = new Dictionary<ICode, BlockInitInfo>();

        protected override ICode VisitCil(StmtCil s) {
            var bsi = this.blockStartInfos[s];
            var stack = new Stack<Expr>(bsi.Stack);
            var locals = bsi.Locals.Cast<Expr>().ToArray();
            var args = bsi.Args.Cast<Expr>().ToArray();
            var cil = new CilProcessor(this.method, stack, locals, args, this.instResults);
            var stmts = new List<Stmt>();
            foreach (var inst in s.Insts) {
                var stmt = cil.Process(inst);
                if (stmt != null) {
                    stmts.Add(stmt);
                }
            }
            // Merge phi's
            // TODO: How to handle 'catch' and 'finally' blocks? How to know which version of each variable to use?
            // Will using phi vars work? Probably.
            var continuations = VisitorFindContinuations.Get(s);
            foreach (var continuation in continuations) {
                var toCil = continuation.To;
                while (toCil.StmtType == Stmt.NodeType.Try) {
                    toCil = ((StmtTry)toCil).Try;
                }
                if (this.blockStartInfos.TryGetValue(toCil, out bsi)) {
                    Action<ExprVarPhi[], IEnumerable<Expr>> merge = (bsiVars, thisVars) => {
                        foreach (var v in bsiVars.Zip(thisVars, (a, b) => new { phi = a, add = b })) {
                            Func<Expr, IEnumerable<Expr>> flattenPhiExprs = null;
                            flattenPhiExprs = e => {
                                if (e.ExprType == Expr.NodeType.VarPhi) {
                                    return ((ExprVarPhi)e).Exprs.SelectMany(x => flattenPhiExprs(x));
                                } else {
                                    return new[] { e };
                                }
                            };
                            v.phi.Exprs = flattenPhiExprs(v.add).Concat(v.phi.Exprs).Where(x => x != v.phi).Distinct().ToArray();
                       }
                    };
                    merge(bsi.Stack, stack);
                    merge(bsi.Args, args);
                    merge(bsi.Locals, locals);
                } else {
                    Func<IEnumerable<Expr>, ExprVarPhi[]> gen = exprs => exprs.Select(x => {
                        if (x.ExprType == Expr.NodeType.VarPhi) {
                            return (ExprVarPhi)x;
                        } else {
                            return new ExprVarPhi(this.method) { Exprs = new[] { x } };
                        }
                    }).ToArray();
                    this.blockStartInfos.Add(toCil, new BlockInitInfo {
                        Stack = gen(stack),
                        Locals = gen(locals),
                        Args = gen(args),
                    });
                }
            }
            // End
            var next = (Stmt)this.Visit(s.EndCil);
            stmts.Add(next);
            return new StmtBlock(stmts);
        }

    }
}
