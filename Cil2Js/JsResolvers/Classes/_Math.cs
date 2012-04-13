using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Math {

        [Js]
        public static Expr Abs(ICall call) {
            var ctx = call.Ctx;
            var arg = call.Arg(0);
            return new ExprJsResolvedMethod(ctx, arg.Type, null, "Math.abs", arg);
        }

        [Js]
        public static Expr Sqrt(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.sqrt", call.Args);
        }

        [Js]
        public static Expr Sin(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.sin", call.Args);
        }

        [Js]
        public static Expr Cos(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.cos", call.Args);
        }

        [Js]
        public static Expr Tan(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.tan", call.Args);
        }

    }
}
