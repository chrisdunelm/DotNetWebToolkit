using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.JsResolvers {
    static class EnvironmentResolver {

        public static Expr GetResourceFromDefault(ICall call) {
            var ctx = call.Ctx;
            var pre = new ExprLiteral(ctx, "GetResourceFromDefault_", ctx.String);
            var expr = new ExprBinary(ctx, BinaryOp.Add, ctx.String, pre, call.Args.First());
            return expr;
        }

    }
}
