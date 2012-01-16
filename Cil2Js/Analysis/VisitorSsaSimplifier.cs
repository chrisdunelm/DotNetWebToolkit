using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
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
                    } else if (a.count == 2 || IsSimple(a.assignment.Expr)) {
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

            private AssignmentInfo GetAInfo(ExprVar e) {
                return this.assignments.ValueOrDefault(e, () => new AssignmentInfo(), true);
            }

            protected override ICode VisitAssignment(StmtAssignment s) {
                var aInfo = this.GetAInfo(s.Target);
                aInfo.assignment = s;
                if (VisitorFindSpecials.Any(s.Expr, Expr.Special.PossibleSideEffects)) {
                    aInfo.mustKeep = true;
                }
                this.Visit(s.Target);
                this.Visit(s.Expr);
                return s;
            }

            private int inPhiCount = 0;

            protected override ICode VisitVarPhi(ExprVarPhi e) {
                // Variables within phi's cannot be removed
                this.inPhiCount++;
                var ret = base.VisitVarPhi(e);
                this.inPhiCount--;
                return ret;
            }

            protected override ICode VisitVarLocal(ExprVarLocal e) {
                var aInfo = this.GetAInfo(e);
                aInfo.count++;
                if (this.inPhiCount > 0) {
                    aInfo.mustKeep = true;
                }
                return base.VisitVarLocal(e);
            }

        }

    }
}
