using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {

    public class VisitorGetVars : JsAstVisitor {

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
