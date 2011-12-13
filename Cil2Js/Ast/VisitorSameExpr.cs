using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Ast {
    public static class VisitorSameExpr {

        public static bool AreSame(Expr a, Expr b, bool exact = false) {
            // exact: Don't allow swapped left/right parameters, or boolean operations
            //if (SameWithDeMorgans(a, b)) {
            //    return true;
            //}
            if (a.ExprType != b.ExprType) {
                return false;
            }
            switch (a.ExprType) {
            case Expr.NodeType.Literal:
                var aLit = (ExprLiteral)a;
                var bLit = (ExprLiteral)b;
                if (aLit.Type.FullName != bLit.Type.FullName) {
                    return false;
                }
                if (aLit.Value == null && bLit.Value == null) {
                    return true;
                }
                if (aLit.Value == null || bLit.Value == null) {
                    return false;
                }
                return aLit.Value.Equals(bLit.Value);
            case Expr.NodeType.Unary:
                var aUnary = (ExprUnary)a;
                var bUnary = (ExprUnary)b;
                return aUnary.Op == bUnary.Op && AreSame(aUnary.Expr, bUnary.Expr);
            case Expr.NodeType.Binary:
                var aBin = (ExprBinary)a;
                var bBin = (ExprBinary)b;
                if (aBin.Op != bBin.Op) {
                    return false;
                }
                if (AreSame(aBin.Left,bBin.Left) && AreSame(aBin.Right,bBin.Right)){
                    return true;
                }
                if (!exact) {
                    // These operations are commutative
                    switch (aBin.Op) {
                    case BinaryOp.Or:
                    case BinaryOp.And:
                    case BinaryOp.BitwiseAnd:
                    case BinaryOp.BitwiseOr:
                    case BinaryOp.BitwiseXor:
                    case BinaryOp.NotEqual:
                        return AreSame(aBin.Left, bBin.Right) && AreSame(aBin.Right, bBin.Left);
                    default:
                        return false;
                    }
                } else {
                    return false;
                }
            case Expr.NodeType.VarExprInstResult:
                var aInst = (ExprVarInstResult)a;
                var bInst = (ExprVarInstResult)b;
                return aInst.Inst == bInst.Inst;
            case Expr.NodeType.VarLocal:
                var aVLoc = (ExprVarLocal)a;
                var bVLoc = (ExprVarLocal)b;
                // Slightly convoluted test required because it is possible to create a local var
                // without a real variable backing it
                return aVLoc == bVLoc || (aVLoc.Var != null && aVLoc.Var == bVLoc.Var);
            case Expr.NodeType.VarParameter:
                var aVParam = (ExprVarParameter)a;
                var bVParam = (ExprVarParameter)b;
                return aVParam.Parameter == bVParam.Parameter;
            case Expr.NodeType.VarPhi:
                return a == b;
            default:
                throw new NotImplementedException("Cannot handle: " + a.ExprType);
            }
        }

        //private static bool SameWithDeMorgans(Expr a, Expr b) {
        //    return Tuple.Create(a, b).Perms((x, y) => {
        //        if (x.ExprType == Expr.NodeType.Binary && y.ExprType == Expr.NodeType.Unary) {
        //            var xBin = (ExprBinary)x;
        //            var yUn = (ExprUnary)y;
        //            if (yUn.Op == UnaryOp.Not && yUn.Expr.ExprType == Expr.NodeType.Binary) {
        //                var yUnBin = (ExprBinary)yUn.Expr;
        //                if ((xBin.Op == BinaryOp.Add && yUnBin.Op == BinaryOp.Or) || (xBin.Op == BinaryOp.Or && yUnBin.Op == BinaryOp.And)) {
        //                    return Tuple.Create(xBin.Left, xBin.Right).Perms((m, n) => {
        //                        return m.DoesEqualNot(yUnBin.Left) && n.DoesEqualNot(yUnBin.Right);
        //                    });
        //                }
        //            }
        //        }
        //        return false;
        //    });
        //}

    }
}
