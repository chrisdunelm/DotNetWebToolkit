using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Single {

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

        [JsRedirect(typeof(Single))]
        public override bool Equals(object obj) {
            throw new JsImplException();
        }
        [Js(typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var type = new ExprJsTypeVarName(ctx, ctx.Single).Named("type");
            return new StmtJsExplicit(ctx, "return other._ === type && this === other.v;", ctx.ThisNamed, other, type);
        }

        public static bool Equals([JsFakeThis]Single _this, Single other) {
            return _this == other;
        }

        public static int CompareTo([JsFakeThis]Single _this, Single other) {
            if (_this < other) {
                return -1;
            }
            if (_this > other) {
                return 1;
            }
            if (_this == other) {
                return 0;
            }
            if (double.IsNaN(_this)) {
                return 1;
            }
            if (double.IsNaN(other)) {
                return -1;
            }
            return 0;
        }

        public static int CompareTo([JsFakeThis]Single _this, object other) {
            if (other == null) {
                return 1;
            }
            if (!(other is Single)) {
                throw new ArgumentException();
            }
            return _this.CompareTo((Single)other);
        }

    }
}
