using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Double {

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

        public static int CompareTo([JsFakeThis]double _this, double other) {
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

    }
}
