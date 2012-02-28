using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorIfSimplification : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorIfSimplification(ast);
            var ret = v.Visit(ast);
            foreach (var replace in v.replaceVars) {
                ret = VisitorReplace.V(ret, replace.Item1, replace.Item2);
            }
            return ret;
        }

        private VisitorIfSimplification(ICode c) {
            this.phiClusters = VisitorPhiClusters.V(c);
        }

        private IEnumerable<IEnumerable<ExprVar>> phiClusters;
        private List<Tuple<ExprVar, ExprVar>> replaceVars = new List<Tuple<ExprVar, ExprVar>>();

        private bool AreClustered(ExprVar a, ExprVar b) {
            return phiClusters.Any(x => x.Contains(a) && x.Contains(b));
        }

        protected override ICode VisitIf(StmtIf s) {
            var then = (Stmt)this.Visit(s.Then);
            var @else = (Stmt)this.Visit(s.Else);
            if (then == null && @else == null) {
                return null;
            }
            // Remove 'if' if condition is just true or false
            if (s.Condition.ExprType == Expr.NodeType.Literal) {
                if (s.Condition.IsLiteralBoolean(true)) {
                    return then;
                }
                if (s.Condition.IsLiteralBoolean(false)) {
                    return @else;
                }
            }
            // If 'then' and 'else' are identical, then remove 'if'
            if (then.DoesEqual(@else)) {
                return then;
            }
            // If 'if' only has an 'else' case, not a 'then' case, then swap
            if (then == null) {
                return new StmtIf(s.Ctx, s.Ctx.ExprGen.NotAutoSimplify(s.Condition), @else, null);
            }
            // If both 'if' parts only contain an assignment to the same (with phi clustering) target, then turn into ternary assignment
            if (then != null && @else != null && then.StmtType == Stmt.NodeType.Assignment && @else.StmtType == Stmt.NodeType.Assignment) {
                var thenAssign = (StmtAssignment)then;
                var elseAssign = (StmtAssignment)@else;
                if (this.AreClustered(thenAssign.Target, elseAssign.Target)) {
                    this.replaceVars.Add(Tuple.Create(elseAssign.Target, thenAssign.Target));
                    var ternary = new ExprTernary(s.Ctx, s.Condition, thenAssign.Expr, elseAssign.Expr);
                    return new StmtAssignment(s.Ctx, thenAssign.Target, ternary);
                }
            }
            // If 'if' contains only 'if' then combine condition with 'and'
            if (@else == null && then.StmtType == Stmt.NodeType.If) {
                var thenIf = (StmtIf)then;
                if (thenIf.Else == null) {
                    return new StmtIf(s.Ctx, s.Ctx.ExprGen.And(s.Condition, thenIf.Condition), thenIf.Then, null);
                }
            }
            if (then != s.Then || @else != s.Else) {
                return new StmtIf(s.Ctx, s.Condition, then, @else);
            } else {
                return s;
            }
        }

        protected override ICode VisitBlock(StmtBlock s) {
            var statements = s.Statements
                .Select(x => (Stmt)this.Visit(x))
                .Where(x => x != null)
                .ToArray();
            var stNew = statements.Combine((a, b) => {
                if (a.StmtType == Stmt.NodeType.If && b.StmtType == Stmt.NodeType.If) {
                    var aIf = (StmtIf)a;
                    var bIf = (StmtIf)b;
                    if (aIf.Then.StmtType != Stmt.NodeType.Continuation && bIf.Then.StmtType != Stmt.NodeType.Continuation) {
                        // Join adjacent 'if' statements if possible. Do not do if both 'if' statements only contain
                        // continuations, so as not to mess up the if-distribution
                        if (aIf.Condition.DoesEqual(bIf.Condition)) {
                            return new StmtIf(s.Ctx, aIf.Condition,
                                new StmtBlock(s.Ctx, aIf.Then, bIf.Then),
                                new StmtBlock(s.Ctx, aIf.Else, bIf.Else));
                        } else if (aIf.Condition.DoesEqualNot(bIf.Condition)) {
                            return new StmtIf(s.Ctx, aIf.Condition,
                                new StmtBlock(s.Ctx, aIf.Then, bIf.Else),
                                new StmtBlock(s.Ctx, aIf.Else, bIf.Then));
                        }
                    }
                    if (aIf.Then.DoesEqual(bIf.Then) && aIf.Else.DoesEqual(bIf.Else)) {
                        // Both 'if' statements contain the same bodies, so 'or' conditions together
                        return new StmtIf(s.Ctx, s.Ctx.ExprGen.Or(aIf.Condition, bIf.Condition), aIf.Then, aIf.Else);
                    }
                } else if (a.StmtType == Stmt.NodeType.If && b.StmtType == Stmt.NodeType.Continuation) {
                    var aIf = (StmtIf)a;
                    if (aIf.Then.DoesEqual(b) || aIf.Else.DoesEqual(b)) {
                        return b;
                    }
                }
                return null;
            }).ToArray();
            if (!Enumerable.SequenceEqual(s.Statements, stNew)) {
                return new StmtBlock(s.Ctx, stNew);
            } else {
                return s;
            }
        }

    }
}
