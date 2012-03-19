using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Boolean {

        [JsRedirect(typeof(bool))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js("Equals", typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var p0 = new ExprVarParameter(ctx, ctx.MDef.Parameters[0]);
            var stmt = new StmtJsExplicit(ctx, "return this._ === other._ && this.v === other.v;", ctx.ThisNamed, other);
            return stmt;
        }

        [JsRedirect(typeof(bool))]
        public override int GetHashCode() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this ? 1 : 0;", ctx.ThisNamed);
        }

        [JsRedirect(typeof(bool))]
        public override string ToString() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt ToString(Ctx ctx) {
            var trueString = ctx.Literal(bool.TrueString, "trueString");
            var falseString = ctx.Literal(bool.FalseString, "falseString");
            return new StmtJsExplicit(ctx, "return this ? trueString : falseString;", ctx.ThisNamed, trueString, falseString);
        }

        public static int CompareTo([JsFakeThis]bool _this, bool other) {
            return _this == other ? 0 : (_this ? 1 : -1);
        }

    }
}
