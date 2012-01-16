using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.JsResolvers {
    public static partial class JsResolver {

        private static Dictionary<M, Func<Ctx, Stmt>> methodMap = new Dictionary<M, Func<Ctx, Stmt>>(M.ValueEqComparer) {
            { M.Def(TVoid, "System.Array.Clear", TArray, TInt32, TInt32), ArrayResolver.Clear },
        };

        public static Stmt ResolveMethod(Ctx ctx) {
            var m = new M(ctx.MRef);
            var fn = methodMap.ValueOrDefault(m);
            if (fn != null) {
                var resolved = fn(ctx);
                return resolved;
            }
            return null;
        }

    }
}
