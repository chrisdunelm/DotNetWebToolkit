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

        protected override ICode VisitJsInvoke(ExprJsInvoke e) {
            this.types.Add(e.Type);
            return base.VisitJsInvoke(e);
        }

        protected override ICode VisitJsVirtualCall(ExprJsVirtualCall e) {
            // Need to add the type of the base method of the virtual call, so
            // the virtual method table is generated correctly
            this.types.Add(e.CallMethod.DeclaringType);
            return base.VisitJsVirtualCall(e);
        }

        protected override ICode VisitDefaultValue(ExprDefaultValue e) {
            this.types.Add(e.Type);
            return base.VisitDefaultValue(e);
        }

        protected override ICode VisitRuntimeHandle(ExprRuntimeHandle e) {
            if (e.Member is TypeReference) {
                this.types.Add((TypeReference)e.Member);
            }
            return base.VisitRuntimeHandle(e);
        }

        //protected override ICode VisitLiteral(ExprLiteral e) {
        //    switch (e.Type.MetadataType) {
        //    case MetadataType.Int64:
        //        this.types.Add(e.Ctx._Int64);
        //        break;
        //    case MetadataType.UInt64:
        //        this.types.Add(e.Ctx._UInt64);
        //        break;
        //    }
        //    return e;
        //}

    }
}
