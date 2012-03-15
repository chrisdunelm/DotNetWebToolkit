using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    struct _Nullable<T> {

        [Js(".ctor", typeof(void), typeof(GenTypeParam0))]
        public static Expr ctor(ICall call) {
            // Value-type ctors can be called as ctors or as methods. Handle both types
            if (call.Obj == null) {
                return call.Arg(0);
            } else {
                return new ExprAssignment(call.Ctx, (ExprVar)call.Obj, call.Arg(0));
            }
        }

        [Js]
        public static Expr get_HasValue(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsExplicit(ctx, "(this !== null)", ctx.Boolean, call.Obj.Named("this"));
        }

        [Js]
        public static Stmt get_Value(Ctx ctx) {
            var excCtor = ctx.Module.Import(typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes));
            var invalidOperationException = new ExprNewObj(ctx, excCtor);
            var throwInvalidOperationException = new StmtThrow(ctx, invalidOperationException);
            var cond = new ExprJsExplicit(ctx, "(this === null)", ctx.Boolean, ctx.ThisNamed);
            var throwIf = new StmtIf(ctx, cond, throwInvalidOperationException, null);
            var ret = new StmtReturn(ctx, ctx.This);
            var stmt = new StmtBlock(ctx, throwIf, ret);
            return stmt;
        }

        [Js(typeof(GenTypeParam0))]
        public static Expr GetValueOrDefault(ICall call) {
            var ctx = call.Ctx;
            var @this = call.Obj.Named("this");
            var @default = new ExprDefaultValue(ctx, call.Type).Named("default");
            return new ExprJsExplicit(ctx, "this || default", call.Type, @this, @default);
        }

        [Js("GetValueOrDefault", typeof(GenTypeParam0), typeof(GenTypeParam0))]
        public static Expr GetValueOrDefaultParam(ICall call) {
            var ctx = call.Ctx;
            var @default = call.Arg(0, "default");
            var objIsVar = call.Obj.IsVar();
            var temp = objIsVar ? null : ctx.Local(call.Obj.Type);
            var js = objIsVar ? "obj !== null ? obj : default" : "(temp = obj) !== null ? temp : default";
            return new ExprJsExplicit(ctx, js, call.Type, temp.Named("temp"), call.Obj.Named("obj"), @default);
        }

    }

}
