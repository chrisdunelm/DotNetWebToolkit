using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;
using System.Collections;
using DotNetWebToolkit.Attributes;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    abstract class _EqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer {

        [JsRedirect(typeof(EqualityComparer<>))]
        private static EqualityComparer<T> CreateComparer() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt CreateComparer(Ctx ctx) {
            var type = ((GenericInstanceType)ctx.MRef.DeclaringType).GenericArguments[0];
            if (type.IsByte()) {
                throw new Exception();
            }
            var iEquatable = ctx.Module.Import(typeof(IEquatable<>)).MakeGeneric(type);
            if (type.DoesImplement(iEquatable)) {
                var t = Type.GetType("System.Collections.Generic.GenericEqualityComparer`1");
                var compType = ctx.Module.Import(t).MakeGeneric(type);
                var ctor = compType.EnumResolvedMethods().First(x => x.Resolve().IsConstructor);
                var ctorExpr = new ExprNewObj(ctx, ctor);
                return new StmtReturn(ctx, ctorExpr);
            }
            throw new NotImplementedException();
        }

        private static EqualityComparer<T> @default; // Do not assign to null - avoids static constructor
        public static EqualityComparer<T> get_Default() {
            if (@default == null) {
                @default = CreateComparer();
            }
            return @default;
        }

        public abstract bool Equals(T x, T y);
        public abstract int GetHashCode(T obj);

        bool IEqualityComparer.Equals(object x, object y) {
            if (x == y) {
                return true;
            }
            if (x == null || y == null) {
                return false;
            }
            return this.Equals((T)x, (T)y);
        }

        int IEqualityComparer.GetHashCode(object obj) {
            return obj == null ? 0 : this.GetHashCode((T)obj);
        }
    }

}
