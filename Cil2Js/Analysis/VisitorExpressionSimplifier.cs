using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorExpressionSimplifier : AstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorExpressionSimplifier();
            return v.Visit(ast);
        }

        private VisitorExpressionSimplifier() { }

        protected override ICode VisitUnary(ExprUnary e) {
            var op = e.Op;
            var expr = (Expr)this.Visit(e.Expr);

            if (op == UnaryOp.Not) {
                if (expr.ExprType == Expr.NodeType.Binary) {
                    var eBin = (ExprBinary)expr;
                    if (eBin.Op == BinaryOp.Equal) {
                        return e.Ctx.ExprGen.NotEqual(eBin.Left, eBin.Right);
                    }
                    if (eBin.Op == BinaryOp.NotEqual) {
                        return e.Ctx.ExprGen.Equal(eBin.Left, eBin.Right);
                    }
                }
            }

            if (expr != e.Expr) {
                return new ExprUnary(e.Ctx, e.Op, e.Type, expr);
            } else {
                return e;
            }
        }

        protected override ICode VisitBinary(ExprBinary e) {
            var op = e.Op;
            var left = (Expr)this.Visit(e.Left);
            var right = (Expr)this.Visit(e.Right);
            var t = Tuple.Create(left, right);

            if (op == BinaryOp.Equal && TypeCombiner.Combine(e.Ctx, left, right).IsBoolean()) {
                if (left.IsLiteralBoolean(true)) {
                    return right;
                }
                if (right.IsLiteralBoolean(true)) {
                    return left;
                }
                if (left.IsLiteralBoolean(false)) {
                    return e.Ctx.ExprGen.NotAutoSimplify(right);
                }
                if (right.IsLiteralBoolean(false)) {
                    return e.Ctx.ExprGen.NotAutoSimplify(left);
                }
            }

            if (left != e.Left || right != e.Right) {
                return new ExprBinary(e.Ctx, op, e.Type, left, right);
            } else {
                return e;
            }
        }

        protected override ICode VisitTernary(ExprTernary e) {
            var condition = (Expr)this.Visit(e.Condition);
            var ifTrue = (Expr)this.Visit(e.IfTrue);
            var ifFalse = (Expr)this.Visit(e.IfFalse);

            if (condition.IsLiteralBoolean(true)) {
                return ifTrue;
            }
            if (condition.IsLiteralBoolean(false)) {
                return ifFalse;
            }

            if (e.Type.IsBoolean()) {
                if (ifTrue.IsLiteralBoolean(true)) {
                    return e.Ctx.ExprGen.Or(condition, ifFalse);
                }
                if (ifTrue.IsLiteralBoolean(false)) {
                    return e.Ctx.ExprGen.And(e.Ctx.ExprGen.NotAutoSimplify(condition), ifFalse);
                }
            }

            if (condition.ExprType == Expr.NodeType.Unary) {
                var cUn = (ExprUnary)condition;
                if (cUn.Op == UnaryOp.Not) {
                    // Remove 'not' from condition and swap ifTrue and ifFalse,
                    return new ExprTernary(e.Ctx, cUn.Expr, ifFalse, ifTrue);
                }
            }

            if (condition != e.Condition || ifTrue != e.IfTrue || ifFalse != e.IfFalse) {
                return new ExprTernary(e.Ctx, condition, ifTrue, ifFalse);
            } else {
                return e;
            }
        }

    }
}
