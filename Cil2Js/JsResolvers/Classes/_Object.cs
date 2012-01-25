using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    
    [Js("ToString", typeof(string))]
    class _Object {

        private _Object() { }

        private static int hashCode = 0;
        [Js(".ctor", typeof(void))]
        public static Stmt ctor(Ctx ctx) {
            Expression<Func<int>> eHashCode = () => hashCode;
            var field = (FieldInfo)((MemberExpression)eHashCode.Body).Member;
            var f = ctx.Module.Import(field);
            var fExpr = new ExprFieldAccess(ctx, null, f);
            var stmt = new StmtJsExplicit(ctx, "{0}.$=++{1};", ctx.This, fExpr);
            return stmt;
        }

        [Js]
        public static Stmt GetType(Ctx ctx) {
            var js = "return typeof({0})==\"string\"?{1}:{0}._";
            var stringType = new ExprJsTypeVarName(ctx, ctx.String);
            var stmt = new StmtJsExplicit(ctx, js, ctx.This, stringType);
            return stmt;
        }

        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var p0 = ctx.MethodParameter(0);
            var stmt = new StmtJsExplicit(ctx, "return {0}==={1};", ctx.This, p0);
            return stmt;
        }

        //[Js(typeof(bool), typeof(object), typeof(object))]
        //public static Expr Equals(ICall call) {
        //    var ctx =call.Ctx;
        //    var a = call.Args.ElementAt(0);
        //    var b = call.Args.ElementAt(1);
        //    var expr = new ExprJsExplicit(ctx, "{0}==={1}", ctx.Boolean, a, b);
        //    return expr;
        //}

        [Js(typeof(bool), typeof(object), typeof(object))]
        public static Expr Equals(ICall call) {
            var a = call.Args.ElementAt(0);
            var b = call.Args.ElementAt(1);
            return new ExprBinary(call.Ctx, BinaryOp.Equal, call.Ctx.Boolean, a, b);
        }

    }
}
