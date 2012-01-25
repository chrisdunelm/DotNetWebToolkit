using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    [Js("ToString", typeof(string))]
    [Js("Concat", typeof(string), typeof(object), typeof(object))]
    [Js("Concat", typeof(string), typeof(object), typeof(object), typeof(object))]
    class _String {

        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var p0 = new ExprVarParameter(ctx, ctx.MDef.Parameters[0]);
            var stmt = new StmtJsExplicit(ctx, "return {0}==={1};", ctx.This, p0);
            return stmt;
        }

        [Js(typeof(int))]
        public static Stmt GetHashCode(Ctx ctx) {
            var acc = new ExprVarLocal(ctx, ctx.Int32);
            var i = new ExprVarLocal(ctx, ctx.Int32);
            var js = "{1}=5381;for({2}=Math.min({0}.length-1,100);{2}>=0;{2}--) {1}=(({1}<<5)+{1}+{0}.charCodeAt({2}))&0x7fffffff; return {1};";
            var stmt = new StmtJsExplicit(ctx, js, ctx.This, acc, i);
            return stmt;
        }

        [Js(typeof(int))]
        public static Expr get_Length(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedProperty(ctx, ctx.Int32, call.Obj, "length");
        }

        [Js(typeof(char), typeof(int))]
        public static Expr get_Chars(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Char, call.Obj, "charCodeAt", call.Args.First());
        }

        [Js(typeof(string), typeof(string), typeof(string))]
        [Js(typeof(string), typeof(string), typeof(string), typeof(string))]
        [Js(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string))]
        public static Expr Concat(ICall call) {
            var expr = call.Args.Aggregate((a, b) => call.Ctx.ExprGen.Add(a, b));
            return expr;
        }

        [Js("Concat", typeof(string), typeof(string[]))]
        public static Expr ConcatStringsArray(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Args.First(), "join", new ExprLiteral(ctx, "", ctx.String));
        }

        [Js("Concat", typeof(string), typeof(object[]))]
        public static string ConcatObjectArray(object[] args) {
            var sb = new StringBuilder();
            foreach (var arg in args) {
                sb.Append(arg);
            }
            return sb.ToString();
        }

        public static string Concat(params object[] args) {
            var sb = new _StringBuilder();
            foreach (var arg in args) {
                sb.Append(arg);
            }
            return sb.ToString();
        }

        [Js("IndexOf", typeof(int), typeof(char))]
        [Js("IndexOf", typeof(int), typeof(char), typeof(int))]
        public static Expr IndexOfChar(ICall call) {
            var ctx = call.Ctx;
            var args = call.Args.Select((x, i) => i != 0 ? x : new ExprJsResolvedMethod(ctx, ctx.Char, null, "String.fromCharCode", x));
            var expr = new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", args);
            return expr;
        }

        [Js("IndexOf", typeof(int), typeof(string))]
        [Js("IndexOf", typeof(int), typeof(string), typeof(int))]
        public static Expr IndexOfString(ICall call) {
            var ctx = call.Ctx;
            var expr = new ExprJsResolvedMethod(ctx, ctx.Int32, call.Obj, "indexOf", call.Args);
            return expr;
        }

        [Js]
        public static Expr Substring(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.String, call.Obj, "substr", call.Args);
        }

        [Js]
        public static Expr op_Equality(ICall call) {
            var left = call.Args.ElementAt(0);
            var right = call.Args.ElementAt(1);
            var expr = call.Ctx.ExprGen.Equal(left, right);
            return expr;
        }

    }
}
