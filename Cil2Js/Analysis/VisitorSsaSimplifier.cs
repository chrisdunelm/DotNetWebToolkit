using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public class VisitorSsaSimplifier : AstVisitor {

        public static ICode V(ICode c) {
            c = CopyPropagation.V(c);
            return c;
        }

        class CopyPropagation : AstVisitor {

            class AssignmentRemover : AstVisitor {

                public AssignmentRemover(ExprVar target) {
                    this.target = target;
                }

                private ExprVar target;

                protected override ICode VisitAssignment(StmtAssignment s) {
                    if (s.Target == this.target) {
                        return null;
                    } else {
                        return base.VisitAssignment(s);
                    }
                }

            }

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
                    if (a.count == 1 || IsSimple(a.assignment.Expr)) {
                        c2 = (new Updater(a.assignment.Target)).Visit(c2);
                        // Remove assignment
                        //c2 = (new AssignmentRemover(a.assignment.Target)).Visit(c2);
                        // Replace target uses with direct expression
                        //c2 = VisitorReplace.V(c2, a.assignment.Target, a.assignment.Expr);
                        //var aInfo = v.assignments.ValueOrDefault(a.assignment.Expr);
                        //if (aInfo != null) {

                        //}
                        //break;
                    }
                }
                return c2;
            }

            class AssignmentInfo {
                public StmtAssignment assignment;
                public int count = 0;
                public bool mustKeep = false;
            }

            private Dictionary<ExprVarLocal, AssignmentInfo> assignments = new Dictionary<ExprVarLocal, AssignmentInfo>();

            protected override ICode VisitAssignment(StmtAssignment s) {
                var aInfo = this.assignments.ValueOrDefault((ExprVarLocal)s.Target, () => new AssignmentInfo(), true);
                aInfo.assignment = s;
                this.Visit(s.Expr);
                return s;
            }

            protected override ICode VisitVarPhi(ExprVarPhi e) {
                // Variables within phi's cannot be removed
                foreach (var expr in e.Exprs.Where(x => x.ExprType == Expr.NodeType.VarLocal).Cast<ExprVarLocal>()) {
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

        }

    }
}
