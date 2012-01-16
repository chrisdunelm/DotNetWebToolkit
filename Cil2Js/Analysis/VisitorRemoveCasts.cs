using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorRemoveCasts : AstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorRemoveCasts();
            return v.Visit(ast);
        }

        private VisitorRemoveCasts() { }

        protected override Ast.ICode VisitCast(ExprCast e) {
            var expr = (Expr)this.Visit(e.Expr);
            switch (expr.ExprType) {
            case Expr.NodeType.Literal:
                return this.Convert((ExprLiteral)expr, e.Type);
            default:
                return e;
            }
        }

        private Expr Convert(ExprLiteral e, TypeReference convertToType) {
            if (convertToType.IsChar()) {
                if (e.Type.IsInt32()) {
                    return new ExprLiteral(e.Ctx, (char)(int)e.Value, e.Ctx.Char);
                }
            }
            if (convertToType.IsBoolean()) {
                if (e.Type.IsInt32()) {
                    return new ExprLiteral(e.Ctx, ((int)e.Value) != 0, e.Ctx.Boolean);
                }
            }
            if (convertToType.Resolve().IsEnum) {
                return e;
            }
            throw new NotImplementedException("Cannot convert");
        }

    }
}
