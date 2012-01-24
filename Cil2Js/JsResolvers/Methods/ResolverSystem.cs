﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Methods {
    static class ResolverSystem {

        private static int hashCode = 0;
        public static Stmt Object_Ctor(Ctx ctx) {
            Expression<Func<int>> eHashCode = () => hashCode;
            var field = (FieldInfo)((MemberExpression)eHashCode.Body).Member;
            var f = ctx.Module.Import(field);
            var fExpr = new ExprFieldAccess(ctx, null, f);
            var stmt = new StmtJsExplicit(ctx, "{0}.$=++{1};", ctx.This, fExpr);
            return stmt;
        }

        public static Stmt Object_GetType(Ctx ctx) {
            var js = "return typeof({0})==\"string\"?{1}:{0}._";
            var stringType = new ExprJsTypeVarName(ctx, ctx.String);
            var stmt = new StmtJsExplicit(ctx, js, ctx.This, stringType);
            return stmt;
        }

        public static Stmt Object_GetHashCode(Ctx ctx) {
            var stmt = new StmtJsExplicit(ctx, "return {0}.$;", ctx.This);
            return stmt;
        }

        public static Stmt Object_Equals(Ctx ctx) {
            var p0 = new ExprVarParameter(ctx, ctx.MDef.Parameters[0]);
            var stmt = new StmtJsExplicit(ctx, "return {0}==={1};", ctx.This, p0);
            return stmt;
        }

        public static Stmt TrivialBoxedValueType_Equals(Ctx ctx) {
            var p0 = new ExprVarParameter(ctx, ctx.MDef.Parameters[0]);
            var stmt = new StmtJsExplicit(ctx, "return {0}._==={1}._&&{0}.v==={1}.v;", ctx.This, p0);
            return stmt;
        }

        public static Stmt TrivialValueType_Equals(Ctx ctx) {
            var p0 = new ExprVarParameter(ctx, ctx.MDef.Parameters[0]);
            var stmt = new StmtJsExplicit(ctx, "return {0}==={1};", ctx.This, p0);
            return stmt;
        }

        public static Stmt IntPtrCtor(Ctx ctx) {
            var field = ctx.TDef.Fields.Where(x => !x.IsStatic).Single();
            var stmt = new StmtAssignment(ctx,
                new ExprFieldAccess(ctx, ctx.This, field),
                new ExprVarParameter(ctx, ctx.MRef.Parameters.First()));
            return stmt;
        }

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

        public static Stmt Int32_GetHashCode(Ctx ctx) {
            var stmt = new StmtReturn(ctx, ctx.This);
            return stmt;
        }

        public static Expr RuntimeTypeHandle_IsInterface(ICall call) {
            var ctx = call.Ctx;
            var runtimeType = call.Args.First();
            var eTypeData = new ExprJsTypeData(ctx, TypeData.IsInterface);
            var expr = new ExprJsExplicit(ctx, "{0}.{1}", ctx.Boolean, runtimeType, eTypeData);
            return expr;
        }

        public static Stmt NumberFormatInfo_get_CurrentInfo(Ctx ctx) {
            var v = new ExprLiteral(ctx, null, ctx.Module.Import(typeof(NumberFormatInfo)));
            return new StmtReturn(ctx, v);
        }

    }
}