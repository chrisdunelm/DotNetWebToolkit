using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    static class ResolverSystem {

        public static Expr ActionFunc_ctor(ICall call) {
            var ctx = call.Ctx;
            var _this = call.Args.ElementAt(0);
            var mRef = ((ExprMethodReference)call.Args.ElementAt(1)).Method;
            var args = mRef.Parameters.Select(x => new ExprVarLocal(ctx, x.ParameterType)).ToArray();
            var boundCall = new ExprCall(ctx, mRef, _this, args, false);
            var innerStmt = boundCall.Type.IsVoid() ?
                (Stmt)new StmtWrapExpr(ctx, boundCall) :
                (Stmt)new StmtReturn(ctx, boundCall);
            Expr e = new ExprJsFunction(ctx, args, innerStmt);
            return e;
        }

        public static Expr ActionFunc_Invoke(ICall call) {
            var ctx = call.Ctx;
            Expr e = new ExprJsInvoke(ctx, call.Obj, call.Args, call.Type);
            return e;
        }

        public static Expr ObjectEquals(ICall call) {
            // TODO: This doesn't handle value types
            var ctx = call.Ctx;
            var e = ctx.ExprGen.Equal(call.Obj, call.Args.First());
            return e;
        }

        public static Stmt IntPtrCtor(Ctx ctx, List<TypeReference> newTypesSeen) {
            var field = ctx.TDef.Fields.Where(x => !x.IsStatic).Single();
            var stmt = new StmtAssignment(ctx,
                new ExprFieldAccess(ctx, ctx.This, field),
                new ExprVarParameter(ctx, ctx.MRef.Parameters.First()));
            return stmt;
        }

        public static Stmt Object_GetType(Ctx ctx, List<TypeReference> newTypesSeen) {
            var js = "return typeof({0})==\"string\"?{1}:{0}.{2}";
            var stringType = new ExprJsTypeVarName(ctx, ctx.String);
            var vTable = new ExprJsTypeData(ctx, TypeData.VTable);
            var stmt = new StmtJsExplicitFunction(ctx, js, ctx.This, stringType, vTable);
            var typeRuntimeType = Type.GetType("System.RuntimeType");
            var tt = ctx.Module.Import(typeRuntimeType);
            newTypesSeen.Add(tt);
            return stmt;
        }

    }
}
