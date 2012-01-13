using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorFindSpecials : AstVisitor {

        public static bool Any(ICode ast, Expr.Special specials) {
            return Get(ast, specials).Any();
        }

        public static IEnumerable<Expr> Get(ICode ast, Expr.Special specials) {
            var v = new VisitorFindSpecials(specials);
            v.Visit(ast);
            return v.results;
        }

        private VisitorFindSpecials(Expr.Special specials) {
            this.specials = specials;
        }

        private Expr.Special specials;
        private List<Expr> results = new List<Expr>();

        protected override ICode VisitExpr(Expr e) {
            if ((e.Specials & this.specials) != 0) {
                this.results.Add(e);
            }
            return base.VisitExpr(e);
        }

    }
}
