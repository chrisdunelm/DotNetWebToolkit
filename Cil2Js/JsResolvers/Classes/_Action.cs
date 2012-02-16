using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Action {

        [Js(".ctor", typeof(void), typeof(object), typeof(IntPtr))]
        public static Expr ctor(ICall call) {
            var ctx = call.Ctx;
            var mRef = ((ExprMethodReference)call.Args.ElementAt(1)).Method;
            //if (!mRef.HasThis) {
            //    return call.Args.ElementAt(1);
            //}
            var _this = mRef.HasThis ? call.Args.ElementAt(0) : null;
            var args = mRef.Parameters.Select(x => new ExprVarLocal(ctx, x.ParameterType)).ToArray();
            var boundCall = new ExprCall(ctx, mRef, _this, args, false);
            var innerStmt = boundCall.Type.IsVoid() ?
                (Stmt)new StmtWrapExpr(ctx, boundCall) :
                (Stmt)new StmtReturn(ctx, boundCall);
            Expr e = new ExprJsFunction(ctx, args, innerStmt);
            return e;
        }

        [Js]
        public static Expr Invoke(ICall call) {
            var ctx = call.Ctx;
            Expr e = new ExprJsInvoke(ctx, call.Obj, call.Args, call.Type);
            return e;
        }

    }
}
