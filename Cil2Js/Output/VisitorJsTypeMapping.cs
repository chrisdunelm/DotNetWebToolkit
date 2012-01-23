using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class VisitorJsTypeMapping : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsTypeMapping();
            return v.Visit(ast);
        }

        protected override ICode VisitCall(ExprCall e) {
            var resolvedType = JsResolver.ResolveType(e.CallMethod.DeclaringType);
            if (resolvedType == null) {
                return base.VisitCall(e);
            }
            var mappedMethod = resolvedType.EnumResolvedMethods().First(x => x.MatchMethodOnly(e.CallMethod));
            var expr = new ExprCall(e.Ctx, mappedMethod, e.Obj, e.Args, e.IsVirtualCall, e.ConstrainedType);
            return expr;
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var resolvedType = JsResolver.ResolveType(e.CallMethod.DeclaringType);
            if (resolvedType == null) {
                return base.VisitNewObj(e);
            }
            var mappedMethod = resolvedType.EnumResolvedMethods().First(x => x.MatchMethodOnly(e.CallMethod));
            var expr = new ExprNewObj(e.Ctx, mappedMethod, e.Args);
            return expr;
        }

    }
}
