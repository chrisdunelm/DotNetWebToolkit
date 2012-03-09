using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class VisitorFindFieldAccesses : JsAstVisitor {

        public static IEnumerable<FieldReference> V(ICode ast) {
            var v = new VisitorFindFieldAccesses();
            v.Visit(ast);
            return v.fieldAccesses;
        }

        private List<FieldReference> fieldAccesses = new List<FieldReference>();

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            this.fieldAccesses.Add(e.Field);
            return base.VisitFieldAccess(e);
        }

        protected override ICode VisitFieldAddress(ExprFieldAddress e) {
            this.fieldAccesses.Add(e.Field);
            return base.VisitFieldAddress(e);
        }

    }
}
