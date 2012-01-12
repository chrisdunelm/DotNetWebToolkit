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

        public static ICode V(ICode ast, TypeReference tRef, MethodReference mRef) {
            var v = new VisitorConvertCilToSsa(ast, tRef, mRef);
            ast = v.Visit(ast);
            // Convert all InstResults to normal Vars
            //var vFindInstResults = new VisitorFindInstResults();
            //vFindInstResults.Visit(c);
            foreach (var instResult in v.instResults.Values) {//vFindInstResults.instResults) {
                var var = new ExprVarLocal(ast.Ctx, instResult.Type);
                ast = VisitorReplace.V(ast, instResult, var);
            }
            return ast;
        }

        private VisitorConvertCilToSsa(ICode root, TypeReference tRef, MethodReference mRef) {
            this.ctx = root.Ctx;
            var v = new VisitorFindInstResults();
            v.Visit(root);
            this.instResults = v.instResults.ToDictionary(x => x.Inst);
            this.CreateOrMergeBsi((Stmt)root, new ExprVarPhi[0],
                this.ctx.MDef.Body.Variables.Select(x => (Expr)null).ToArray(),
                this.ctx.MRef.Parameters.Select(x =>
                    (Expr)new ExprVarParameter(this.ctx, x)
                    ).ToArray());
        }

        class BlockInitInfo {
            public ExprVarPhi[] Stack;
            public ExprVarPhi[] Locals;
            public ExprVarPhi[] Args;
        }

        private Ctx ctx;
        private Dictionary<Instruction, ExprVarInstResult> instResults;
        private Dictionary<ICode, BlockInitInfo> blockStartInfos = new Dictionary<ICode, BlockInitInfo>();

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
                    v.phi.Exprs = flattenPhiExprs(v.add).Concat(v.phi.Exprs).Where(x => x != null && x != v.phi).Distinct().ToArray();
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
                this.blockStartInfos.Add(s, new BlockInitInfo {
                    Stack = create(stack),
                    Locals = create(locals),
                    Args = create(args),
                });
            } else {
                merge(bsi.Stack, stack);
                merge(bsi.Locals, locals);
                merge(bsi.Args, args);
            }
        }

        protected override ICode VisitCil(StmtCil s) {
            var bsi = this.blockStartInfos[s];
            var stack = new Stack<Expr>(bsi.Stack.Reverse());
            var locals = bsi.Locals.Cast<Expr>().ToArray();
            var args = bsi.Args.Cast<Expr>().ToArray();
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
