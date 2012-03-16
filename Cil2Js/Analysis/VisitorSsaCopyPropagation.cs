using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorSsaCopyPropagation : AstVisitor {

        // TODO: Restrict movement across try/catch/finally borders

        public static ICode V(ICode ast) {
            var v = new VisitorSsaCopyPropagation();
            var ast2 = v.Visit(ast);
            var alreadyReplaced = new Dictionary<Expr, Expr>();
            foreach (var a in v.assignments.Values) {
                if (a.mustKeep) {
                    continue;
                }
                if (a.assignment == null) {
                    continue;
                }
                if (a.count == 1) {
                    if (!VisitorFindSpecials.Any(a.assignment, Expr.Special.PossibleSideEffects)) {
                        ast2 = VisitorReplace.V(ast2, a.assignment, null);
                    }
                } else if (a.count == 2 || 
                    (IsSimple(alreadyReplaced.ValueOrDefault(a.assignment.Expr, a.assignment.Expr)) && !a.assignment.Expr.Type.IsNonPrimitiveValueType())) {
                    var updater = new Updater(a.assignment.Target);
                    ast2 = updater.Visit(ast2);
                    foreach (var replaced in updater.Replaced) {
                        alreadyReplaced[replaced.Item1] = replaced.Item2;
                    }
                }
            }
            return ast2;
        }

        class Updater : AstVisitor {

            public Updater(ExprVar target) {
                this.target = target;
            }

            private ExprVar target;
            private Expr replaceWith = null;
            private List<Tuple<Expr, Expr>> replaced = new List<Tuple<Expr, Expr>>();

            public IEnumerable<Tuple<Expr, Expr>> Replaced { get { return this.replaced; } }

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
                    this.replaced.Add(Tuple.Create(e, this.replaceWith));
                    return this.replaceWith;
                } else {
                    return base.VisitExpr(e);
                }
            }

        }

        private static bool IsSimple(Expr e) {
            return
                e.ExprType == Expr.NodeType.VarLocal ||
                e.ExprType == Expr.NodeType.VarParameter ||
                e.ExprType == Expr.NodeType.VarThis;
        }

        class AssignmentInfo {
            public StmtAssignment assignment;
            public int count = 0;
            public bool mustKeep = false;
        }

        private Dictionary<ExprVar, AssignmentInfo> assignments = new Dictionary<ExprVar, AssignmentInfo>();
        private int inPhiCount = 0;

        private AssignmentInfo GetAInfo(ExprVar e) {
            return this.assignments.ValueOrDefault(e, () => new AssignmentInfo(), true);
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            var aInfo = this.GetAInfo(s.Target);
            aInfo.assignment = s;
            if (s.Target.ExprType != Expr.NodeType.VarLocal || s.Target.Type.IsByReference ||
                VisitorFindSpecials.Any(s.Expr, Expr.Special.PossibleSideEffects)) {
                aInfo.mustKeep = true;
            }
            this.Visit(s.Target);
            this.Visit(s.Expr);
            return s;
        }

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
