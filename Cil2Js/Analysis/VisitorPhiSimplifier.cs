using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    class VisitorPhiSimplifier : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorPhiSimplifier();
            return v.Visit(ast);
        }

        protected override ICode VisitVarPhi(ExprVarPhi e) {
            if (e.Exprs.Count() == 1) {
                return this.Visit(e.Exprs.First());
            }
            var exprNew = e.Exprs.Select(x => (Expr)this.Visit(x)).ToArray();
            var exprs = exprNew
                .SelectMany(x => {
                    if (x == null) {
                        return Enumerable.Empty<Expr>();
                    }
                    if (x.ExprType == Expr.NodeType.VarPhi) {
                        return ((ExprVarPhi)x).Exprs;
                    }
                    return new[] { x };
                })
                .Distinct(Expr.EqComparerExact)
                .ToArray();
            if (exprs.SequenceEqual(e.Exprs)) {
                return e;
            } else {
                return new ExprVarPhi(e.Ctx) { Exprs = exprs };
            }
        }

    }
}
