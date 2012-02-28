using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
//using Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public static class VisitorSameExpr {

        public static bool AreSame(Expr a, Expr b, bool exact) {
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
                return aUnary.Op == bUnary.Op && AreSame(aUnary.Expr, bUnary.Expr, exact);
            case Expr.NodeType.Binary:
                var aBin = (ExprBinary)a;
                var bBin = (ExprBinary)b;
                if (aBin.Op != bBin.Op) {
                    return false;
                }
                if (AreSame(aBin.Left, bBin.Left, exact) && AreSame(aBin.Right, bBin.Right, exact)) {
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
                    case BinaryOp.Equal:
                    case BinaryOp.Add:
                    case BinaryOp.Mul:
                        return AreSame(aBin.Left, bBin.Right, exact) && AreSame(aBin.Right, bBin.Left, exact);
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
            case Expr.NodeType.VarParameter:
            case Expr.NodeType.VarPhi:
                return a == b;
            case Expr.NodeType.FieldAccess:
                return ((ExprFieldAccess)a).Field.Resolve() == ((ExprFieldAccess)b).Field.Resolve();
            default:
                return a == b;
                //throw new NotImplementedException("Cannot handle: " + a.ExprType);
            }
        }

    }
}
