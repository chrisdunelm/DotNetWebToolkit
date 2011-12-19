using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public class VisitorSsaSimplifier : AstVisitor {

        public static ICode V(ICode ast) {
            ast = CopyPropagation.V(ast);
            return ast;
        }

        class CopyPropagation : AstVisitor {

            class Updater : AstVisitor {

                public Updater(ExprVar target) {
                    this.target = target;
                }

                private ExprVar target;
                private Expr replaceWith = null;

                protected override ICode VisitAssignment(StmtAssignment s) {
                    if (s.Target == this.target) {
                        this.replaceWith = s.Expr;
                        return null;
                    } else {
                        return base.VisitAssignment(s);
                    }
                }

                protected override ICode VisitExpr(Expr e) {
                    if (e == this.target) {
                        if (this.replaceWith == null) {
                            throw new InvalidOperationException("This should never occur");
                        }
                        return this.replaceWith;
                    } else {
                        return base.VisitExpr(e);
                    }
                }

            }

            private static bool IsSimple(Expr e) {
                return false;
            }

            public static ICode V(ICode c) {
                var v = new CopyPropagation();
                var c2 = v.Visit(c);
                foreach (var a in v.assignments.Values) {
                    if (a.mustKeep) {
                        continue;
                    }
                    if (a.count == 1) {
                        if (!VisitorFindSpecials.Any(a.assignment, Expr.Special.PossibleSideEffects)) {
                            c2 = VisitorReplace.V(c2, a.assignment, null);
                        }
                    }
                    if (a.count == 2 || IsSimple(a.assignment.Expr)) {
                        c2 = (new Updater(a.assignment.Target)).Visit(c2);
                    }
                }
                return c2;
            }

            class AssignmentInfo {
                public StmtAssignment assignment;
                public int count = 0;
                public bool mustKeep = false;
            }

            private Dictionary<ExprVar, AssignmentInfo> assignments = new Dictionary<ExprVar, AssignmentInfo>();

            protected override ICode VisitAssignment(StmtAssignment s) {
                var aInfo = this.assignments.ValueOrDefault((ExprVar)s.Target, () => new AssignmentInfo(), true);
                aInfo.assignment = s;
                this.Visit(s.Target);
                this.Visit(s.Expr);
                return s;
            }

            protected override ICode VisitVarPhi(ExprVarPhi e) {
                // Variables within phi's cannot be removed
                foreach (var expr in e.Exprs.Where(x => x.ExprType == Expr.NodeType.VarLocal).Cast<ExprVar>()) {
                    var aInfo = this.assignments.ValueOrDefault(expr, () => new AssignmentInfo(), true);
                    aInfo.mustKeep = true;
                }
                return base.VisitVarPhi(e);
            }

            protected override ICode VisitVarLocal(ExprVarLocal e) {
                var aInfo = this.assignments.ValueOrDefault(e);
                if (aInfo != null) {
                    aInfo.count++;
                }
                return base.VisitVarLocal(e);
            }

            //protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            //    var aInfo = this.assignments.ValueOrDefault(e);
            //    if (aInfo != null) {
            //        aInfo.count++;
            //    }
            //    return base.VisitFieldAccess(e);
            //}

        }

    }
}
