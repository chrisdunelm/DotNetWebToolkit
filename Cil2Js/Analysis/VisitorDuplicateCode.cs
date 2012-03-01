using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    class VisitorDuplicateCode : AstVisitor {

        public static ICode V(ICode ast) {
            var ctx = ast.Ctx;
            var blockInfo = VisitorSubstituteIrreducable.FindSuitableBlocks.GetInfo(ast);
            var bInfo = blockInfo.Select(x => new {
                toCount = VisitorContToCounter.GetCount(x.Key, ast),
                ast = x.Key,
                codeCount = x.Value.numICodes,
            }).ToArray();
            var block = bInfo.Where(x => x.toCount >= 2).OrderBy(x => x.toCount * x.codeCount).FirstOrDefault();
            if (block == null) {
                return ast;
            }

            var phis = new Dictionary<Expr,Expr>();
            var blockCopies = Enumerable.Range(0, block.toCount - 1)
                .Select(x => {
                    var v = new VisitorDuplicateCode();
                    var ret = v.Visit(block.ast);
                    phis = phis.Merge(v.phiMap, (a, b) => new ExprVarPhi(ctx) { Exprs = new[] { a, b } });
                    return ret;
                })
                .Concat(block.ast)
                .ToArray();

            var contTos = VisitorFindContinuationsRecursive.Get(ast).Where(x => x.To == block.ast).ToArray();
            for (int i = 0; i < contTos.Length; i++) {
                var contTo = contTos[i];
                var blockCopy = blockCopies[i];
                ast = VisitorReplace.V(ast, contTo, blockCopy);
            }

            ast = VisitorReplaceExprUse.V(ast, phis);

            return ast;
        }

        private VisitorDuplicateCode() : base(true) { }

        private Dictionary<Expr, Expr> phiMap = new Dictionary<Expr, Expr>();

        protected override ICode VisitBlock(StmtBlock s) {
            return new StmtBlock(s.Ctx, s.Statements.Select(x => (Stmt)this.Visit(x)));
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            var ctx = s.Ctx;
            var newTarget = ctx.Local(s.Target.Type);
            var phi = new ExprVarPhi(ctx) { Exprs = new[] { s.Target, newTarget } };
            this.phiMap.Add(s.Target, phi);
            return new StmtAssignment(s.Ctx, newTarget, (Expr)this.Visit(s.Expr));
        }

        protected override ICode VisitReturn(StmtReturn s) {
            return new StmtReturn(s.Ctx, (Expr)this.Visit(s.Expr));
        }

        protected override ICode VisitContinuation(StmtContinuation s) {
            return new StmtContinuation(s.Ctx, s.To, s.LeaveProtectedRegion);
        }

        protected override ICode VisitExpr(Expr e) {
            return e;
        }

    }
}
