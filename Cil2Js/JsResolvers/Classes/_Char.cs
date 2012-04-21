using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Char {

        [JsRedirect(typeof(char))]
        public override string ToString() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt ToString(Ctx ctx) {
            var js = "return String.fromCharCode(this);";
            var stmt = new StmtJsExplicit(ctx, js, ctx.ThisNamed);
            return stmt;
        }

        [JsRedirect(typeof(char))]
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this | this << 16;", ctx.ThisNamed);
        }

        [JsRedirect(typeof(Char))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.Char).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
        }

        public static bool Equals([JsFakeThis]Char _this, Char other) {
            return _this == other;
        }

        public static int CompareTo([JsFakeThis]char _this, char other) {
            return (int)(_this - other);
        }

        public static int CompareTo([JsFakeThis]char _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is char)) {
                throw new ArgumentException();
            }
            return (int)(_this - (char)other);
        }

    }
}
