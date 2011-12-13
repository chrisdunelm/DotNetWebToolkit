using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorFindSpecials : AstVisitor {

        public static bool Any(ICode ast, params Expr.Special[] specials) {
            return Get(ast, specials).Any();
        }

        public static IEnumerable<Expr> Get(ICode ast, params Expr.Special[] specials) {
            var v = new VisitorFindSpecials(specials);
            v.Visit(ast);
            return v.results;
        }

        private VisitorFindSpecials(IEnumerable<Expr.Special> specials) {
            this.specials = specials;
        }

        private IEnumerable<Expr.Special> specials;
        private List<Expr> results = new List<Expr>();

        protected override ICode VisitExpr(Expr e) {
            if (this.specials.Any(x => (e.Specials & x) != 0)) {
                this.results.Add(e);
            }
            return base.VisitExpr(e);
        }

    }
}
