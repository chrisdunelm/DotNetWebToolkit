using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Analysis {
    public class VisitorExpressionSimplifier : AstVisitor {

        public static ICode V(MethodDefinition method, ICode c) {
            var v = new VisitorExpressionSimplifier(method);
            return v.Visit(c);
        }

        private VisitorExpressionSimplifier(MethodDefinition method) {
            this.typeSystem = method.Module.TypeSystem;
            this.exprGen = Expr.ExprGen(this.typeSystem);
        }

        private TypeSystem typeSystem;
        private Expr.Gen exprGen;

        protected override ICode VisitUnary(ExprUnary e) {
            var op = e.Op;
            var expr = (Expr)this.Visit(e.Expr);

            if (op == UnaryOp.Not) {
                if (expr.ExprType == Expr.NodeType.Binary) {
                    var eBin = (ExprBinary)expr;
                    if (eBin.Op == BinaryOp.Equal) {
                        return this.exprGen.NotEqual(eBin.Left, eBin.Right);
                    }
                    if (eBin.Op == BinaryOp.NotEqual) {
                        return this.exprGen.Equal(eBin.Left, eBin.Right);
                    }
                }
            }

            if (expr != e.Expr) {
                return new ExprUnary(e.Op, e.Type, expr);
            } else {
                return e;
            }
        }

        protected override ICode VisitBinary(ExprBinary e) {
            var op = e.Op;
            var left = (Expr)this.Visit(e.Left);
            var right = (Expr)this.Visit(e.Right);
            var t  = Tuple.Create(left,right);

            if (op == BinaryOp.Equal && TypeCombiner.Combine(this.typeSystem, left,right).IsBoolean()) {
                if (left.IsLiteralBoolean(true)) {
                    return right;
                }
                if (right.IsLiteralBoolean(true)) {
                    return left;
                }
                if (left.IsLiteralBoolean(false)) {
                    return this.exprGen.NotAutoSimplify(right);
                }
                if (right.IsLiteralBoolean(false)) {
                    return this.exprGen.NotAutoSimplify(left);
                }
            }

            if (left != e.Left || right != e.Right) {
                return new ExprBinary(op, e.Type, left, right);
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
                    return this.exprGen.Or(condition, ifFalse);
                }
                if (ifTrue.IsLiteralBoolean(false)) {
                    return this.exprGen.And(this.exprGen.NotAutoSimplify(condition), ifFalse);
                }
            }

            if (condition.ExprType == Expr.NodeType.Unary) {
                var cUn = (ExprUnary)condition;
                if (cUn.Op == UnaryOp.Not) {
                    // Remove 'not' from condition and swap ifTrue and ifFalse,
                    return new ExprTernary(e.TypeSystem, cUn.Expr, ifFalse, ifTrue);
                }
            }

            if (condition != e.Condition || ifTrue != e.IfTrue || ifFalse != e.IfFalse) {
                return new ExprTernary(e.TypeSystem, condition, ifTrue, ifFalse);
            } else {
                return e;
            }
        }

    }
}
