using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Output;

namespace Cil2Js.JsResolvers {
    static class SystemResolver {

        public static JsResolved Action_ctor(ICall call) {
            var ctx = call.Ctx;
            var _this = call.Args.ElementAt(0);
            var method = ((ExprMethodReference)call.Args.ElementAt(1)).Method.Resolve();
            var boundCall = new ExprCall(ctx, method, _this, Enumerable.Empty<Expr>(), false);
            var innerStmt = new StmtWrapExpr(ctx, boundCall);
            Expr e = new ExprJsFunction(ctx, innerStmt);
            return new JsResolvedExpr(e);
        }

    }
}
