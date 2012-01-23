using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Methods {
    static class ResolverString {

        public static Expr op_Equality(ICall call) {
            var left = call.Args.ElementAt(0);
            var right = call.Args.ElementAt(1);
            var expr = call.Ctx.ExprGen.Equal(left, right);
            return expr;
        }

        public static Stmt GetHashCode(Ctx ctx) {
            var acc = new ExprVarLocal(ctx, ctx.Int32);
            var i = new ExprVarLocal(ctx, ctx.Int32);
            var js = "{1}=5381;for({2}=Math.min({0}.length-1,100);{2}>=0;{2}--) {1}=(({1}<<5)+{1}+{0}.charCodeAt({2}))&0x7fffffff; return {1};";
            var stmt = new StmtJsExplicit(ctx, js, ctx.This, acc, i);
            return stmt;
        }

        public static Expr get_Length(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedProperty(ctx, ctx.Int32, call.Obj, "length");
        }

        public static Expr get_Chars(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Char, call.Obj, "charCodeAt", call.Args.First());
        }

        public static Expr ConcatStrings(ICall call) {
            var expr = call.Args.Aggregate((a, b) => call.Ctx.ExprGen.Add(a, b));
            return expr;
        }

        public static Expr ConcatStringsMany(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Args.First(), "join", new ExprLiteral(ctx, "", ctx.String));
        }

        public static Expr IndexOfChar(ICall call) {
            var ctx = call.Ctx;
            var args = call.Args.Select((x, i) => i != 0 ? x : new ExprJsResolvedMethod(ctx, ctx.Char, null, "String.fromCharCode", x));
            var expr = new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", args);
            return expr;
        }

        public static Expr IndexOfString(ICall call) {
            var ctx = call.Ctx;
            var expr = new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", call.Args);
            return expr;
        }

        public static Expr Substring(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Obj, "substr", call.Args);
        }

    }
}
