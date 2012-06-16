using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;
using Int8 = System.SByte;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Math {

        [Js]
        public static Stmt Abs(Ctx ctx) {
            var arg = ctx.MethodParameter(0);
            ExprLiteral minVal;
            switch (arg.Type.MetadataType) {
            case MetadataType.SByte: minVal = ctx.Literal(Int8.MinValue); break;
            case MetadataType.Int16: minVal = ctx.Literal(Int16.MinValue); break;
            case MetadataType.Int32: minVal = ctx.Literal(Int32.MinValue); break;
            case MetadataType.Int64: minVal = ctx.Literal(Int64.MinValue); break;
            default: minVal = null; break;
            }
            if (minVal != null) {
                var exCtor = ctx.Module.Import(typeof(OverflowException).GetConstructor(Type.EmptyTypes));
                return new StmtIf(ctx,
                    ctx.ExprGen.Equal(arg, minVal),
                    new StmtThrow(ctx, new ExprNewObj(ctx, exCtor)),
                    new StmtReturn(ctx,
                        arg.Type.IsInt64() ?
                        (Expr)new ExprCall(ctx, (Func<Int64, Int64>)_Int64.Abs, null, arg) :
                        (Expr)new ExprJsResolvedMethod(ctx, arg.Type, null, "Math.abs", arg))
                    );
            } else {
                return new StmtReturn(ctx, new ExprJsResolvedMethod(ctx, arg.Type, null, "Math.abs", arg));
            }
        }

        [Js]
        public static Expr Acos(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.acos", call.Args);
        }

        [Js]
        public static Expr Asin(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.asin", call.Args);
        }

        [Js]
        public static Expr Atan(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.atan", call.Args);
        }

        [Js]
        public static Stmt Atan2(Ctx ctx) {
            var arg0 = ctx.MethodParameter(0);
            var arg1 = ctx.MethodParameter(1);
            var eg = ctx.ExprGen;
            return new StmtReturn(ctx,
                new ExprTernary(ctx,
                    eg.And(
                        eg.Not(new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg0)),
                        eg.Not(new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg1))
                    ),
                    ctx.Literal(Double.NaN),
                    new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.atan2", arg0, arg1)
                )
            );
        }

        public static long BigMul(int a, int b) {
            return (long)a * (long)b;
        }

        [Js]
        public static Stmt Ceiling(Ctx ctx) {
            var arg = ctx.MethodParameter(0);
            var e = new ExprTernary(ctx,
                new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg),
                new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.ceil", arg),
                arg);
            return new StmtReturn(ctx, e);
        }

        [Js]
        public static Stmt Cos(Ctx ctx) {
            var arg = ctx.MethodParameter(0);
            var eg = ctx.ExprGen;
            return new StmtReturn(ctx,
                new ExprTernary(ctx,
                    new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg),
                    new ExprTernary(ctx,
                        eg.And(
                            eg.GreaterThan(arg, new ExprJsExplicit(ctx, long.MinValue.ToString(), ctx.Int64)),
                            eg.LessThan(arg, new ExprJsExplicit(ctx, long.MaxValue.ToString(), ctx.Int64))
                        ),
                        new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.cos", arg),
                        arg
                    ),
                    ctx.Literal(Double.NaN)
                )
            );
        }

        public static double Cosh(double value) {
            return (Math.Exp(value) + Math.Exp(-value)) / 2.0;
        }

        public static int DivRem(int a, int b, out int result) {
            result = a % b;
            return a / b;
        }

        public static long DivRem(long a, long b, out long result) {
            result = a % b;
            return a / b;
        }

        [Js]
        public static Expr Exp(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.exp", call.Args);
        }

        [Js]
        public static Stmt Floor(Ctx ctx) {
            var arg = ctx.MethodParameter(0);
            var e = new ExprTernary(ctx,
                new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg),
                new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.floor", arg),
                arg);
            return new StmtReturn(ctx, e);
        }

        [Js(typeof(double), typeof(double))]
        public static Expr Log(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.log", call.Args);
        }

        public static double Log(double a, double newBase) {
            return Math.Log(a) / Math.Log(newBase);
        }

        public static double Log10(double d) {
            return Math.Log(d, 10.0);
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
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.pow", call.Args);
        }

        [Js]
        public static Expr Round(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.round", call.Args);
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
            if (Single.IsNaN(a)) {
                throw new ArithmeticException();
            }
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        public static int Sign(Double a) {
            if (Double.IsNaN(a)) {
                throw new ArithmeticException();
            }
            return a > 0 ? 1 : (a < 0 ? -1 : 0);
        }

        [Js]
        public static Stmt Sin(Ctx ctx) {
            var arg = ctx.MethodParameter(0);
            var eg = ctx.ExprGen;
            return new StmtReturn(ctx,
                new ExprTernary(ctx,
                    new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg),
                    new ExprTernary(ctx,
                        eg.And(
                            eg.GreaterThan(arg, new ExprJsExplicit(ctx, long.MinValue.ToString(), ctx.Int64)),
                            eg.LessThan(arg, new ExprJsExplicit(ctx, long.MaxValue.ToString(), ctx.Int64))
                        ),
                        new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.sin", arg),
                        arg
                    ),
                    ctx.Literal(Double.NaN)
                )
            );
        }

        public static double Sinh(double value) {
            return (Math.Exp(value) - Math.Exp(-value)) / 2.0;
        }

        [Js]
        public static Expr Sqrt(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.sqrt", call.Args);
        }

        [Js]
        public static Stmt Tan(Ctx ctx) {
            var arg = ctx.MethodParameter(0);
            var eg = ctx.ExprGen;
            return new StmtReturn(ctx,
                new ExprTernary(ctx,
                    eg.And(
                        new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "Number.isFinite", arg),
                        eg.And(
                            eg.GreaterThan(arg, new ExprJsExplicit(ctx, long.MinValue.ToString(), ctx.Int64)),
                            eg.LessThan(arg, new ExprJsExplicit(ctx, long.MaxValue.ToString(), ctx.Int64))
                        )
                    ),
                    new ExprJsResolvedMethod(ctx, ctx.Double, null, "Math.tan", arg),
                    ctx.Literal(Double.NaN)
                )
            );
        }

        public static double Tanh(double value) {
            var e2 = Math.Exp(2.0 * value);
            if (double.IsPositiveInfinity(e2)) {
                return 1.0;
            }
            return (e2 - 1.0) / (e2 + 1.0);
        }

    }
}
