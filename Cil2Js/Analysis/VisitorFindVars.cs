using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {

    public class VisitorFindVars : AstVisitor {

        public static IEnumerable<ExprVar> V(ICode ast) {
            var v = new VisitorFindVars();
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
