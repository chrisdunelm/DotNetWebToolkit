using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class VisitorFindNewArrays : JsAstVisitor {

        public static IEnumerable<TypeReference> V(ICode ast) {
            var v = new VisitorFindNewArrays();
            v.Visit(ast);
            return v.newArrayTypes;
        }

        private List<TypeReference> newArrayTypes = new List<TypeReference>();

        protected override ICode VisitNewArray(ExprNewArray e) {
            newArrayTypes.Add(e.Type);
            return base.VisitNewArray(e);
        }

    }
}
