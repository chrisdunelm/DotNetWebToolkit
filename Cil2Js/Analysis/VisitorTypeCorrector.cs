using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public class VisitorTypeCorrector : AstVisitor {

        public static ICode V(ICode ast) {
            var nowReplace = new Dictionary<ExprVar, ExprVarLocal>();
            for (; ; ) {
                var v = new VisitorTypeCorrector {
                    nowReplace = nowReplace
                    //thisForceToBoolean = new HashSet<ExprVarLocal>(forceToBoolean)
                };
                ast = v.Visit(ast);
                if (!v.toReplace.Any()) {
                    return ast;
                }
                //forceToBoolean = v.newforceToBoolean;
                nowReplace = v.toReplace;
            }
        }

        private VisitorTypeCorrector() { }

        private Dictionary<ExprVar, ExprVarLocal> nowReplace;
        private Dictionary<ExprVar, ExprVarLocal> toReplace = new Dictionary<ExprVar, ExprVarLocal>();

        //private HashSet<ExprVarLocal> thisForceToBoolean;
        //private List<ExprVarLocal> newforceToBoolean = new List<ExprVarLocal>();

        protected override ICode VisitAssignment(StmtAssignment s) {
            if (this.nowReplace.ContainsKey(s.Target)) {
                var expr = s.Expr;
                if (!s.Expr.Type.IsBoolean()) {
                    expr = this.ConvertToBoolean(s.Expr);
                }
                return new StmtAssignment(s.Ctx, this.nowReplace[s.Target], expr);
            }
            return base.VisitAssignment(s);
        }

        //protected override ICode VisitAssignment(StmtAssignment s) {
        //    if (this.thisForceToBoolean.Contains(s.Target)) {
        //        if (!s.Expr.Type.IsBoolean()) {
        //            if (s.Expr.ExprType == Expr.NodeType.VarLocal) {
        //                this.newforceToBoolean.Add((ExprVarLocal)s.Expr);
        //            } else if (s.Expr.ExprType == Expr.NodeType.Literal) {
        //                var eExprLiteral = (ExprLiteral)s.Expr;
        //                if (s.Expr.Type.IsInt32()) {
        //                    return new StmtAssignment(s.Ctx, s.Target, new ExprLiteral(s.Ctx, ((int)eExprLiteral.Value) != 0 ? true : false, s.Ctx.Boolean));
        //                }
        //            }
        //        }
        //    }
        //    return s;
        //}

        //protected override ICode VisitAssignment(ExprAssignment e) {
        //    if (this.thisForceToBoolean.Contains(e.Target)) {
        //        if (!e.Expr.Type.IsBoolean()) {
        //            if (e.Expr.ExprType == Expr.NodeType.VarLocal) {
        //                this.newforceToBoolean.Add((ExprVarLocal)e.Expr);
        //            } else if (e.Expr.ExprType == Expr.NodeType.Literal) {
        //                var eExprLiteral = (ExprLiteral)e.Expr;
        //                if (e.Expr.Type.IsInt32()) {
        //                    return new ExprAssignment(e.Ctx, e.Target, new ExprLiteral(e.Ctx, ((int)eExprLiteral.Value) != 0 ? true : false, e.Ctx.Boolean));
        //                }
        //            }
        //        }
        //    }
        //    return e;
        //}

        private Expr ConvertToBoolean(Expr e) {
            if (e.ExprType == Expr.NodeType.Literal) {
                var eLiteral = (ExprLiteral)e;
                if (eLiteral.Type.IsInt32()) {
                    return new ExprLiteral(e.Ctx, ((int)eLiteral.Value) != 0 ? true : false, e.Ctx.Boolean);
                }
            }
            if (e.ExprType == Expr.NodeType.VarLocal) {
                var var = new ExprVarLocal(e.Ctx, e.Ctx.Boolean);
                this.toReplace.Add((ExprVarLocal)e, var);
                return var;
            }
            throw new InvalidOperationException();
        }

        protected override ICode VisitVarPhi(ExprVarPhi e) {
            if (e.Type.IsBoolean()) {
                List<Expr> newExprs = null;
                foreach (var expr in e.Exprs) {
                    var mustConvert = !expr.Type.IsBoolean();
                    var newExpr = mustConvert ? this.ConvertToBoolean(expr) : expr;
                    if (mustConvert && newExprs == null) {
                        newExprs = new List<Expr>(e.Exprs.TakeWhile(x => x != expr));
                    }
                    if (newExprs != null) {
                        newExprs.Add(newExpr);
                    }
                }
                if (newExprs != null) {
                    return new ExprVarPhi(e.Ctx) { Exprs = newExprs };
                }
            }
            return e;
        }

        protected override ICode VisitBinary(ExprBinary e) {
            if (e.Op == BinaryOp.Equal || e.Op == BinaryOp.NotEqual) {
                if (e.Right.Type.IsBoolean() && !e.Left.Type.IsBoolean()) {
                    return new ExprBinary(e.Ctx, e.Op, e.Type, this.ConvertToBoolean(e.Left), e.Right);
                    //if (e.Left.ExprType != Expr.NodeType.VarLocal) {
                    //    throw new InvalidOperationException("VarLoval expected");
                    //}
                    //this.newforceToBoolean.Add((ExprVarLocal)e.Left);
                } else if (e.Left.Type.IsBoolean() && !e.Right.Type.IsBoolean()) {
                    return new ExprBinary(e.Ctx, e.Op, e.Type, e.Left, this.ConvertToBoolean(e.Right));
                    //this.MarkToConvertToBoolean(e.Right);
                    //if (e.Right.ExprType != Expr.NodeType.VarLocal) {
                    //    throw new InvalidOperationException("VarLoval expected");
                    //}
                    //this.newforceToBoolean.Add((ExprVarLocal)e.Right);
                }
            }
            return base.VisitBinary(e);
        }

    }
}
