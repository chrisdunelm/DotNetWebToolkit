using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Analysis {
    public class VisitorRemoveCasts : AstVisitor {

        public static ICode V(TypeSystem typeSystem, ICode ast) {
            var v = new VisitorRemoveCasts(typeSystem);
            return v.Visit(ast);
        }

        private VisitorRemoveCasts(TypeSystem typeSystem) {
            this.typeSystem = typeSystem;
        }

        private TypeSystem typeSystem;

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
                    return new ExprLiteral((char)(int)e.Value, this.typeSystem.Char);
                }
            }
            if (convertToType.IsBoolean()) {
                if (e.Type.IsInt32()) {
                    return new ExprLiteral(((int)e.Value) == 0, this.typeSystem.Boolean);
                }
            }
            throw new NotImplementedException("Cannot convert");
        }

    }
}
