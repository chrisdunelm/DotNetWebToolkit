using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorReplaceExprUse : AstRecursiveVisitor {

        public static ICode V(ICode ast, Expr find, Expr replace) {
            var v = new VisitorReplaceExprUse(new Dictionary<Expr, Expr> { { find, replace } });
            return v.Visit(ast);
        }

        public static ICode V(ICode ast, Dictionary<Expr, Expr> replace) {
            var v = new VisitorReplaceExprUse(replace);
            return v.Visit(ast);
        }

        private VisitorReplaceExprUse(Dictionary<Expr, Expr> replace) {
            this.replace = replace;
        }

        private Dictionary<Expr, Expr> replace;

        protected override ICode VisitExpr(Expr e) {
            Expr r;
            if (this.replace.TryGetValue(e, out r)) {
                return r;
            }
            return base.VisitExpr(e);
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            var expr = (Expr)this.Visit(s.Expr);
            if (expr != s.Expr) {
                return new StmtAssignment(s.Ctx, s.Target, expr);
            } else {
                return s;
            }
        }

        protected override ICode VisitAssignment(ExprAssignment e) {
            var expr = (Expr)this.Visit(e.Expr);
            if (expr != e.Expr) {
                return new ExprAssignment(e.Ctx, e.Target, expr);
            } else {
                return e;
            }
        }

    }
}
