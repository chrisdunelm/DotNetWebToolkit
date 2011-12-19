using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    static class TypeCombiner {

        public static TypeReference Combine(Ctx ctx, Expr a, Expr b) {
            return Combine(ctx, a.Type, b.Type);
        }

        public static TypeReference Combine(Ctx ctx, TypeReference a, TypeReference b) {
            if (a.IsSame(b)) {
                return a;
            }
            var t = Tuple.Create(a, b);
            if (t.Perms((x, y) => x.IsBoolean() && y.IsInt32())) {
                return ctx.Boolean;
            }
            return ctx.Object;
        }

    }
}
