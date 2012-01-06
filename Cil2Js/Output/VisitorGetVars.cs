using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public class VisitorGetVars : AstVisitor {

        public static IEnumerable<ExprVar> V(ICode ast) {
            var v = new VisitorGetVars();
            v.Visit(ast);
            return v.vars;
        }

        private List<ExprVar> vars = new List<ExprVar>();

        protected override ICode VisitVar(ExprVar e) {
            this.vars.Add(e);
            return e;
        }

    }

}
