using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    static class ResolverType {

        public static Stmt cctor(Ctx ctx, List<TypeReference> newTypesSeen) {
            return new StmtEmpty(ctx);
        }

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

        public static Stmt IsArrayImpl(Ctx ctx, List<TypeReference> newTypesSeen) {
            var eIsArray = new ExprJsTypeData(ctx, TypeData.IsArray);
            var stmt = new StmtJsExplicitFunction(ctx, "return {0}.{1};", ctx.This, eIsArray);
            return stmt;
        }

        //public static Stmt GetInterfaces(Ctx ctx, List<TypeReference> newTypesSeen) {
        //    var eInterfaces = new ExprJsTypeData(ctx, TypeData.Interfaces);
        //    var ret = new ExprVarLocal(ctx, ctx.Type);
        //    var typeArrayType = ctx.Type.MakeArray();
        //    var arrayType = new ExprJsTypeVarName(ctx, typeArrayType);
        //    var js = "{2}={0}.{1}.slice(0); {2}._={3}; return {2};";
        //    var stmt = new StmtJsExplicitFunction(ctx,js, ctx.This, eInterfaces, ret, arrayType);
        //    newTypesSeen.Add(typeArrayType);
        //    return stmt;
        //}

        public static Stmt GetAttributeFlagsImpl(Ctx ctx, List<TypeReference> newTypesSeen) {
            var stmt = new StmtJsExplicitFunction(ctx, "return 0;");
            return stmt;
        }

    }
}
