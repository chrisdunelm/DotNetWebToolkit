using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Double : IFormattable {

        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            var toString = new ExprJsResolvedMethod(ctx, ctx.String, ctx.This, "toExponential");
            var getHashCode = new ExprCall(ctx, typeof(string).GetMethod("GetHashCode"), toString);
            var stmt = new StmtReturn(ctx, getHashCode);
            return stmt;
        }

        [Js]
        public static Expr IsNaN(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Boolean, null, "isNaN", call.Args);
        }

        public static int CompareTo([JsFakeThis]Double _this, Double other) {
            if (_this < other) {
                return -1;
            }
            if (_this > other) {
                return 1;
            }
            if (_this == other) {
                return 0;
            }
            if (Double.IsNaN(_this)) {
                return 1;
            }
            if (Double.IsNaN(other)) {
                return -1;
            }
            return 0;
        }

        public static int CompareTo([JsFakeThis]Double _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is Double)) {
                throw new ArgumentException();
            }
            return _this.CompareTo((Double)other);
        }

        [JsRedirect(typeof(Double))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.Double).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
        }

        public static bool Equals([JsFakeThis]Double _this, Double other) {
            return _this == other;
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

        [JsRedirect(typeof(Double))]
        public string ToString(string format, IFormatProvider formatProvider) {
            throw new JsImplException();
        }
        [Js(typeof(string), typeof(string), typeof(IFormatProvider))]
        public static Stmt ToString(Ctx ctx) {
            var value = ctx.This;
            var format = ctx.MethodParameter(0);
            var provider = ctx.MethodParameter(1);
            var nfi = ctx.Literal(null, ctx.Module.Import(typeof(NumberFormatInfo)));
            var expr = new ExprCall(ctx, (Func<double, string, NumberFormatInfo, string>)JsResolvers.Classes.Helpers.Number.FormatDouble, null, value, format, nfi);
            return new StmtReturn(ctx, expr);
        }

    }
}
