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

        public static Expr GetTypeFromHandle(ICall call) {
            return call.Args.First();
        }

        public static Stmt get_BaseType(Ctx ctx, List<TypeReference> newTypesSeen) {
            var tType = ctx.Module.Import(typeof(Type));
            var eBaseType = new ExprJsTypeData(ctx, TypeData.BaseType);
            var stmt = new StmtJsExplicitFunction(ctx, "return {0}.{1};", ctx.This, eBaseType);
            return stmt;
        }

    }
}
