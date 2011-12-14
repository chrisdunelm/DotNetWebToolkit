using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {

    public class VisitorFindStaticConstructors : AstVisitor {

        public static IEnumerable<MethodDefinition> V(ICode ast) {
            var v = new VisitorFindStaticConstructors();
            v.Visit(ast);
            return v.staticConstructors;
        }

        private VisitorFindStaticConstructors() { }

        private List<MethodDefinition> staticConstructors = new List<MethodDefinition>();

        private void Add(TypeDefinition type) {
            var cctor = type.Methods.FirstOrDefault(x => x.Name == ".cctor");
            if (cctor != null) {
                this.staticConstructors.Add(cctor);
            }
        }

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            if (e.IsStatic) {
                this.Add(e.Field.DeclaringType);
            }
            return e;
        }

        protected override ICode VisitCall(ExprCall e) {
            if (e.IsStatic) {
                this.Add(e.CallMethod.DeclaringType);
            }
            return e;
        }

    }

}
