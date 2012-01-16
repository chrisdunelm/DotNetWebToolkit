using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Output;
using Mono.Cecil;

namespace Cil2Js.JsResolvers {
    static class StringResolver {

        public static Expr op_Equality(ICall call) {
            var left = call.Args.ElementAt(0);
            var right = call.Args.ElementAt(1);
            var expr = call.Ctx.ExprGen.Equal(left, right);
            return expr;
        }

        public static Expr get_Length(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedProperty(ctx, ctx.Int32, call.Obj, "length");
        }

        public static Expr get_Chars(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Char, call.Obj, "charAt", call.Args.First());
        }

        public static Expr ConcatStrings(ICall call) {
            var expr = call.Args.Aggregate((a, b) => call.Ctx.ExprGen.Add(a, b));
            return expr;
        }

        public static Expr ConcatStringsMany(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Args.First(), "join", new ExprLiteral(ctx, "", ctx.String));
        }

        public static Expr IndexOf(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", call.Args);
        }

        public static Expr Substring(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Obj, "substr", call.Args);
        }

    }
}
