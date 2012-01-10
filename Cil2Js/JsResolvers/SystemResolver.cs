using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Output;
using Cil2Js.Utils;

namespace Cil2Js.JsResolvers {
    static class SystemResolver {

        public static JsResolved ActionFunc_ctor(ICall call) {
            var ctx = call.Ctx;
            var _this = call.Args.ElementAt(0);
            var mRef = ((ExprMethodReference)call.Args.ElementAt(1)).Method;
            var args = mRef.Parameters.Select(x => new ExprVarLocal(ctx, x.ParameterType)).ToArray();
            var boundCall = new ExprCall(ctx, mRef, _this, args, false);
            var innerStmt = boundCall.Type.IsVoid() ?
                (Stmt)new StmtWrapExpr(ctx, boundCall) :
                (Stmt)new StmtReturn(ctx, boundCall);
            Expr e = new ExprJsFunction(ctx, args, innerStmt);
            return new JsResolvedExpr(e);
        }

        public static JsResolved ActionFunc_Invoke(ICall call) {
            var ctx = call.Ctx;
            Expr e = new ExprJsInvoke(ctx, call.Obj, call.Args, call.Type);
            return new JsResolvedExpr(e);
        }

    }
}
