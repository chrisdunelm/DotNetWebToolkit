using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    [Js("get_FullName", typeof(string))]
    class _Type {

        [Js]
        public static Expr op_Equality(ICall call) {
            var ctx = call.Ctx;
            var a = call.Args.ElementAt(0);
            var b = call.Args.ElementAt(1);
            var expr = new ExprBinary(ctx, BinaryOp.Equal, ctx.Boolean, a, b);
            return expr;
        }

        [Js]
        public static Expr GetTypeFromHandle(ICall call) {
            return call.Args.First();
        }

    }
}
