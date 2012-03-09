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
    
    //[Js("ToString", typeof(string))]
    class _Object {

        private _Object() { }

        private static int hashCode = 0;
        [Js(".ctor", typeof(void))]
        public static Stmt ctor(Ctx ctx) {
            Expression<Func<int>> eHashCode = () => hashCode;
            var field = (FieldInfo)((MemberExpression)eHashCode.Body).Member;
            var f = ctx.Module.Import(field);
            var fExpr = new ExprFieldAccess(ctx, null, f).Named("hashCode");
            var stmt = new StmtJsExplicit(ctx, "this.$=++hashCode;", ctx.ThisNamed, fExpr);
            return stmt;
        }

        [Js]
        public static Stmt GetType(Ctx ctx) {
            var js = "return typeof(this)==\"string\"?stringType:this._||__[this.tagName]||__[this.constructor.name];";
            var stringType = new ExprJsTypeVarName(ctx, ctx.String).Named("stringType");
            var stmt = new StmtJsExplicit(ctx, js, ctx.ThisNamed, stringType);
            return stmt;
        }

        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var stmt = new StmtJsExplicit(ctx, "return this===other;", ctx.ThisNamed, other);
            return stmt;
        }

        [Js(typeof(bool), typeof(object), typeof(object))]
        public static Expr Equals(ICall call) {
            var a = call.Args.ElementAt(0);
            var b = call.Args.ElementAt(1);
            return new ExprBinary(call.Ctx, BinaryOp.Equal, call.Ctx.Boolean, a, b);
        }

        public override string ToString() {
            return this.GetType().ToString();
        }

    }
}
