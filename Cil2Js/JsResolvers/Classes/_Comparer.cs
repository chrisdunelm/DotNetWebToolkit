using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class ComparableComparer<T> : Comparer<T> where T : IComparable<T> {
        public override int Compare(T x, T y) {
            if (x == null) {
                return -1;
            }
            if (y == null) {
                return 1;
            }
            return x.CompareTo(y);
        }
    }

    [Js("get_Default", typeof(Comparer<GenTypeParam0>))]
    class _Comparer<T> {

        public _Comparer() { }

        [Js]
        public static Stmt CreateComparer(Ctx ctx) {
            var type = ((GenericInstanceType)ctx.MRef.DeclaringType).GenericArguments[0];
            var iComparer = ctx.Module.Import(typeof(IComparer<>)).MakeGeneric(type);
            if (type.DoesImplement(iComparer)) {
                var t = Type.GetType("System.Collections.Generic.GenericComparer`1");
                var compType = ctx.Module.Import(t).MakeGeneric(type);
                var ctor = compType.EnumResolvedMethods().First(x => x.Resolve().IsConstructor);
                var ctorExpr = new ExprNewObj(ctx, ctor);
                return new StmtReturn(ctx, ctorExpr);
            }
            var iComparable = ctx.Module.Import(typeof(IComparable<>)).MakeGeneric(type);
            if (type.DoesImplement(iComparable)) {
                var compType = ctx.Module.Import(typeof(ComparableComparer<>)).MakeGeneric(type);
                var ctor = compType.EnumResolvedMethods().First(x => x.Resolve().IsConstructor);
                var ctorExpr = new ExprNewObj(ctx, ctor);
                return new StmtReturn(ctx, ctorExpr);
            }
            return new StmtThrow(ctx, ctx.Literal(null, ctx.Object));
        }

    }

}
