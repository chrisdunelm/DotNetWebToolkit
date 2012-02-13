using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Thread {

        [Js]
        public static Expr get_CurrentThread(ICall call) {
            var ctx = call.Ctx;
            return ctx.Literal(null, ctx.Module.Import(typeof(Thread)));
        }

        [Js]
        public static Expr get_ManagedThreadId(ICall call) {
            return call.Ctx.Literal(0);
        }

    }
}
