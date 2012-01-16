using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class VisitorFindFieldAccesses : AstVisitor {

        public static IEnumerable<FieldReference> V(ICode ast) {
            var v = new VisitorFindFieldAccesses();
            v.Visit(ast);
            return v.fieldAccesses;
        }

        private List<FieldReference> fieldAccesses = new List<FieldReference>();

        protected override Ast.ICode VisitFieldAccess(ExprFieldAccess e) {
            this.fieldAccesses.Add(e.Field);
            return base.VisitFieldAccess(e);
        }

    }
}
