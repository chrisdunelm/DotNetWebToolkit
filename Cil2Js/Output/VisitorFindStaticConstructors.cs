using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {

    public class VisitorFindStaticConstructors : AstVisitor {

        public static IEnumerable<MethodReference> V(ICode ast) {
            var v = new VisitorFindStaticConstructors();
            v.Visit(ast);
            return v.staticConstructors;
        }

        private VisitorFindStaticConstructors() { }

        private List<MethodReference> staticConstructors = new List<MethodReference>();

        private void Add(TypeReference tRef, Ctx ctx) {
            var cctor = tRef.Resolve().Methods.FirstOrDefault(x => x.Name == ".cctor");
            if (cctor != null) {
                this.staticConstructors.Add(cctor.FullResolve(ctx));
            }
        }

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            if (e.IsStatic) {
                this.Add(e.Field.DeclaringType, e.Ctx);
            }
            return e;
        }

    }

}
