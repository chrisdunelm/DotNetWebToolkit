using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Methods {
    static class ResolverType {

        public static Stmt cctor(Ctx ctx) {
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

        public static Stmt get_FullName(Ctx ctx) {
            var eNamespace = new ExprJsTypeData(ctx, TypeData.Namespace);
            var eName = new ExprJsTypeData(ctx, TypeData.Name);
            var stmt = new StmtJsExplicit(ctx, "return {0}.{1}+\".\"+{0}.{2};", ctx.This, eNamespace, eName);
            return stmt;
        }

        public static Stmt ToString(Ctx ctx) {
            var eNamespace = new ExprJsTypeData(ctx, TypeData.Namespace);
            var eName = new ExprJsTypeData(ctx, TypeData.Name);
            var stmt = new StmtJsExplicit(ctx, "return {0}.{1}+\".\"+{0}.{2};", ctx.This, eNamespace, eName);
            return stmt;
        }

    }
}
