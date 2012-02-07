using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;
using DotNetWebToolkit.Cil2Js.Analysis;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public static class Extensions {

        public static bool DoesEqual(this Expr a, Expr b) {
            return VisitorSameExpr.AreSame(a, b, false);
        }

        public static bool DoesEqualExact(this Expr a, Expr b) {
            return VisitorSameExpr.AreSame(a, b, true);
        }

        public static bool DoesEqualNot(this Expr a, Expr b) {
            if (a.ExprType == Expr.NodeType.Unary && ((ExprUnary)a).Op == UnaryOp.Not) {
                return ((ExprUnary)a).Expr.DoesEqual(b);
            }
            if (b.ExprType == Expr.NodeType.Unary && ((ExprUnary)b).Op == UnaryOp.Not) {
                return ((ExprUnary)b).Expr.DoesEqual(a);
            }
            return Tuple.Create(a, b).Perms((x, y) => {
                var xNot = x.Ctx.ExprGen.Not(x);
                return xNot.DoesEqual(y);
            });
        }

        public static bool DoesEqualNotExact(this Expr a, Expr b) {
            if (a.ExprType == Expr.NodeType.Unary && ((ExprUnary)a).Op == UnaryOp.Not) {
                return ((ExprUnary)a).Expr.DoesEqualExact(b);
            }
            if (b.ExprType == Expr.NodeType.Unary && ((ExprUnary)b).Op == UnaryOp.Not) {
                return ((ExprUnary)b).Expr.DoesEqualExact(a);
            }
            return Tuple.Create(a, b).Perms((x, y) => {
                var xNot = x.Ctx.ExprGen.Not(x);
                return xNot.DoesEqualExact(y);
            });
        }

        public static bool IsLiteralBoolean(this Expr e, bool value) {
            if (e.ExprType == Expr.NodeType.Literal) {
                var eLit = (ExprLiteral)e;
                if (eLit.Type.IsBoolean()) {
                    return value == (bool)eLit.Value;
                }
                if (eLit.Type.IsInt32()) {
                    return value == ((int)eLit.Value != 0 ? true : false);
                }
                throw new InvalidOperationException("This literal cannot be treated like a boolean");
            }
            return false;
        }

        public static bool IsLiteralNull(this Expr e) {
            if (e.ExprType == Expr.NodeType.Literal) {
                return ((ExprLiteral)e).Value == null;
            }
            return false;
        }

        public static bool DoesEqual(this Stmt a, Stmt b) {
            return VisitorSameStmt.AreSame(a, b);
        }

    }
}
