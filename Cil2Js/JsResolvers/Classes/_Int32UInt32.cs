using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Int32 : IFormattable {

        [JsRedirect(typeof(Int32))]
        public static int Parse(string s) {
            throw new JsImplException();
        }
        [Js("Parse", typeof(Int32), typeof(string), IsStatic = true)]
        public static Expr Parse(ICall call) {
            var ctx = call.Ctx;
            return new ExprCall(ctx, (Func<string, Int32>)JsResolvers.Classes.Helpers.Number.ParseInt32, null, call.Args.ToArray());
        }

        [JsRedirect(typeof(Int32))]
        public override int GetHashCode() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this;", ctx.ThisNamed);
        }

        [JsRedirect(typeof(Int32))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.Int32).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
        }

        public override string ToString() {
            return this.ToString(null, null);
        }

        public string ToString(string format) {
            return this.ToString(format, null);
        }

        public string ToString(IFormatProvider provider) {
            return this.ToString(null, provider);
        }

        [JsRedirect(typeof(Int32))]
        public string ToString(string format, IFormatProvider formatProvider) {
            throw new JsImplException();
        }
        [Js(typeof(string), typeof(string), typeof(IFormatProvider))]
        public static Stmt ToString(Ctx ctx) {
            var value = ctx.This;
            var format = ctx.MethodParameter(0);
            var provider = ctx.MethodParameter(1);
            var nfi = ctx.Literal(null, ctx.Module.Import(typeof(NumberFormatInfo)));
            var expr = new ExprCall(ctx, (Func<Int32, string, NumberFormatInfo, string>)Cil2Js.JsResolvers.Classes.Helpers.Number.FormatInt32, null, value, format, nfi);
            return new StmtReturn(ctx, expr);
        }

        public static bool Equals([JsFakeThis]Int32 _this, Int32 other) {
            return _this == other;
        }

        public static int CompareTo([JsFakeThis]Int32 _this, Int32 other) {
            return _this < other ? -1 : (_this > other ? 1 : 0);
        }

        public static int CompareTo([JsFakeThis]Int32 _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is Int32)) {
                throw new ArgumentException();
            }
            return _this.CompareTo((Int32)other);
        }

    }

    class _UInt32 : IFormattable {

        [JsRedirect(typeof(Int32))]
        public override int GetHashCode() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return ~~this;", ctx.ThisNamed);
        }

        [JsRedirect(typeof(Int32))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.UInt32).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
        }

        public override string ToString() {
            return this.ToString(null, null);
        }

        public string ToString(string format) {
            return this.ToString(format, null);
        }

        public string ToString(IFormatProvider provider) {
            return this.ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            throw new NotImplementedException();
        }

        public static bool Equals([JsFakeThis]UInt32 _this, UInt32 other) {
            return _this == other;
        }

        public static int CompareTo([JsFakeThis]UInt32 _this, UInt32 other) {
            return _this < other ? -1 : (_this > other ? 1 : 0);
        }

        public static int CompareTo([JsFakeThis]UInt32 _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is UInt32)) {
                throw new ArgumentException();
            }
            return _this.CompareTo((UInt32)other);
        }

    }

}
