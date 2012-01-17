using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    static class ResolverType {

        public static Expr op_Equality(ICall call) {
            var ctx = call.Ctx;
            var a = call.Args.ElementAt(0);
            var b = call.Args.ElementAt(1);
            var expr = new ExprBinary(ctx, BinaryOp.Equal, ctx.Boolean, a, b);
            return expr;
        }

        public static Expr op_Inequality(ICall call) {
            var ctx = call.Ctx;
            var a = call.Args.ElementAt(0);
            var b = call.Args.ElementAt(1);
            var expr = new ExprBinary(ctx, BinaryOp.NotEqual, ctx.Boolean, a, b);
            return expr;
        }

        public static Expr GetTypeFromHandle(ICall call) {
            return call.Args.First();
        }

        public static Stmt get_BaseType(Ctx ctx, List<TypeReference> newTypesSeen) {
            var eBaseType = new ExprJsTypeData(ctx, TypeData.BaseType);
            var stmt = new StmtJsExplicitFunction(ctx, "return {0}.{1};", ctx.This, eBaseType);
            return stmt;
        }

        public static Stmt GetElementType(Ctx ctx, List<TypeReference> newTypesSeen) {
            var eElementType = new ExprJsTypeData(ctx, TypeData.ElementType);
            var stmt = new StmtJsExplicitFunction(ctx, "return {0}.{1};", ctx.This, eElementType);
            return stmt;
        }

        public static Stmt get_IsArray(Ctx ctx, List<TypeReference> newTypesSeen) {
            var eIsArray = new ExprJsTypeData(ctx, TypeData.IsArray);
            var stmt = new StmtJsExplicitFunction(ctx, "return {0}.{1};", ctx.This, eIsArray);
            return stmt;
        }

    }
}
