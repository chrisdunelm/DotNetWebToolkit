using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorFindRequiredTypes : JsAstVisitor {

        public static IEnumerable<TypeReference> V(ICode ast) {
            var v = new VisitorFindRequiredTypes();
            v.Visit(ast);
            return v.types;
        }

        private List<TypeReference> types = new List<TypeReference>();

        protected override ICode VisitBox(ExprBox e) {
            this.types.Add(e.Type);
            return base.VisitBox(e);
        }

        protected override ICode VisitJsTypeVarName(ExprJsTypeVarName e) {
            this.types.Add(e.TypeRef);
            return base.VisitJsTypeVarName(e);
        }

        //override VisitC
        protected override ICode VisitJsInvoke(ExprJsInvoke e) {
            this.types.Add(e.Type);
            return base.VisitJsInvoke(e);
        }
    }
}
