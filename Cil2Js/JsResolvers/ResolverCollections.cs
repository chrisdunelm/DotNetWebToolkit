using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    static class ResolverCollections {

        public static Stmt EqualityComparer_CreateComparer(Ctx ctx, List<TypeReference> _) {
            var type = ((GenericInstanceType)ctx.MRef.DeclaringType).GenericArguments[0];
            if (type.IsByte()) {
                throw new Exception();
            }
            var iEquatable = ctx.Module.Import(typeof(IEquatable<>)).MakeGeneric(type);
            if (type.DoesImplement(iEquatable)) {
                var t = Type.GetType("System.Collections.Generic.GenericEqualityComparer`1");
                var compType = ctx.Module.Import(t).MakeGeneric(type);
                var ctor = compType.EnumResolvedMethods().First(x=>x.Resolve().IsConstructor);
                var ctorExpr = new ExprNewObj(ctx, ctor);
                return new StmtReturn(ctx, ctorExpr);
            }
            throw new Exception();
        }

    }
}
