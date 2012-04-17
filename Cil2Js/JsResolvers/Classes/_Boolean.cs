using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Boolean : IEquatable<Boolean>, IComparable, IComparable<Boolean> {

        [JsRedirect(typeof(bool))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js("Equals", typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.Boolean).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
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

        [JsRedirect(typeof(Boolean))]
        public int CompareTo(bool other) {
            throw new JsImplException();
        }
        public static int CompareTo([JsFakeThis]bool _this, bool other) {
            return _this == other ? 0 : (_this ? 1 : -1);
        }

        [JsRedirect(typeof(Boolean))]
        public int CompareTo(object obj) {
            throw new JsImplException();
        }
        public static int CompareTo([JsFakeThis]bool _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is bool)) {
                throw new ArgumentException();
            }
            return _this == (bool)other ? 0 : (_this ? 1 : -1);
        }

        [JsRedirect(typeof(Boolean))]
        public bool Equals(bool other) {
            throw new JsImplException();
        }
        public static bool Equals([JsFakeThis]bool _this, bool other) {
            return _this == other;
        }

    }
}
