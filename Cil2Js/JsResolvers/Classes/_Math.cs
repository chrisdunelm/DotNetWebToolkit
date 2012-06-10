using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;

using Int8 = System.SByte;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Math {

        [Js]
        public static Expr Abs(ICall call) {
            var ctx = call.Ctx;
            var arg = call.Arg(0);
            if (arg.Type.IsInt64()) {
                return new ExprCall(ctx, (Func<Int64, Int64>)_Int64.Abs, null, arg);
            } else {
                return new ExprJsResolvedMethod(ctx, arg.Type, null, "Math.abs", arg);
            }
        }

        [Js]
        public static Expr Cos(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.cos", call.Args);
        }

        [Js]
        public static Expr Max(ICall call) {
            var ctx = call.Ctx;
            var arg0 = call.Arg(0);
            var arg1 = call.Arg(1);
            if (arg0.Type.IsInt64()) {
                return new ExprCall(ctx, (Func<Int64, Int64, Int64>)_Int64.Max, null, arg0, arg1);
            } else if (arg0.Type.IsUInt64()) {
                return new ExprCall(ctx, (Func<UInt64, UInt64, UInt64>)_UInt64.Max, null, arg0, arg1);
            } else {
                return new ExprJsResolvedMethod(ctx, arg0.Type, null, "Math.max", arg0, arg1);
            }
        }

        [Js]
        public static Expr Min(ICall call) {
            var ctx = call.Ctx;
            var arg0 = call.Arg(0);
            var arg1 = call.Arg(1);
            if (arg0.Type.IsInt64()) {
                return new ExprCall(ctx, (Func<Int64, Int64, Int64>)_Int64.Min, null, arg0, arg1);
            } else if (arg0.Type.IsUInt64()) {
                return new ExprCall(ctx, (Func<UInt64, UInt64, UInt64>)_UInt64.Min, null, arg0, arg1);
            } else {
                return new ExprJsResolvedMethod(ctx, arg0.Type, null, "Math.min", arg0, arg1);
            }
        }

        [Js]
        public static Expr Pow(ICall call) {
            var ctx = call.Ctx;
            var arg0 = call.Arg(0);
            var arg1 = call.Arg(1);
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.pow", arg0, arg1);
        }

        public static int Sign(Int8 a) {
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        public static int Sign(Int16 a) {
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        public static int Sign(Int32 a) {
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        public static int Sign(Int64 a) {
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        public static int Sign(Single a) {
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        public static int Sign(Double a) {
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        [Js]
        public static Expr Sin(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.sin", call.Args);
        }

        [Js]
        public static Expr Sqrt(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.sqrt", call.Args);
        }

        [Js]
        public static Expr Tan(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.tan", call.Args);
        }

    }
}
