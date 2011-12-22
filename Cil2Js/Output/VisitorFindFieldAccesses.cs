using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class VisitorFindFieldAccesses : AstVisitor {

        public static IEnumerable<FieldDefinition> V(ICode ast) {
            var v = new VisitorFindFieldAccesses();
            v.Visit(ast);
            return v.fieldAccesses;
        }

        private List<FieldDefinition> fieldAccesses = new List<FieldDefinition>();

        protected override Ast.ICode VisitFieldAccess(ExprFieldAccess e) {
            this.fieldAccesses.Add(e.Field.Resolve());
            return base.VisitFieldAccess(e);
        }

    }
}
