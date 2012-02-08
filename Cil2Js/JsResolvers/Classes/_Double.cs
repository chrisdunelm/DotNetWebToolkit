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

        [Js("return isNaN(a);")]
        public static bool IsNaN(double d) {
            throw new Exception();
        }
    
    }
}
