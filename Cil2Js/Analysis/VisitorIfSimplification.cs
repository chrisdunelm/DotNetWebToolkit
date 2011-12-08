using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
    public class VisitorIfSimplification : AstRecursiveVisitor {

        public static ICode V(MethodDefinition method, ICode c) {
            var v = new VisitorIfSimplification(method, c);
            var ret = v.Visit(c);
            foreach (var replace in v.replaceVars) {
                ret = VisitorReplace.V(ret, replace.Item1, replace.Item2);
            }
            return ret;
        }

        private VisitorIfSimplification(MethodDefinition method, ICode c) {
            this.typeSystem = method.Module.TypeSystem;
            this.exprGen = Expr.ExprGen(this.typeSystem);
            this.phiClusters = VisitorPhiClusters.V(c);
        }

        private TypeSystem typeSystem;
        private Expr.Gen exprGen;
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
                return new StmtIf(new ExprUnary(UnaryOp.Not, this.typeSystem.Boolean, s.Condition), @else);
            }
            // If both 'if' parts only contain an assignment to the same (with phi clustering) target, then turn into ternary assignment
            if (then != null && @else != null && then.StmtType == Stmt.NodeType.Assignment && @else.StmtType == Stmt.NodeType.Assignment) {
                var thenAssign = (StmtAssignment)then;
                var elseAssign = (StmtAssignment)@else;
                if (this.AreClustered(thenAssign.Target, elseAssign.Target)) {
                    this.replaceVars.Add(Tuple.Create(elseAssign.Target, thenAssign.Target));
                    var ternary = new ExprTernary(this.typeSystem, s.Condition, thenAssign.Expr, elseAssign.Expr);
                    return new StmtAssignment(thenAssign.Target, ternary);
                }
            }
            if (then != s.Then || @else != s.Else) {
                return new StmtIf(s.Condition, then, @else);
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
                        // 
                        if (aIf.Condition.DoesEqual(bIf.Condition)) {
                            return new StmtIf(aIf.Condition,
                                new StmtBlock(aIf.Then, bIf.Then),
                                new StmtBlock(aIf.Else, bIf.Else));
                        } else if (aIf.Condition.DoesEqualNot(bIf.Condition)) {
                            return new StmtIf(aIf.Condition,
                                new StmtBlock(aIf.Then, bIf.Else),
                                new StmtBlock(aIf.Else, bIf.Then));
                        }
                    }
                    if (aIf.Then.DoesEqual(bIf.Then) && aIf.Else.DoesEqual(bIf.Else)) {
                        // Both 'if' statements contain the same bodies, so 'or' conditions together
                        return new StmtIf(this.exprGen.Or(aIf.Condition, bIf.Condition), aIf.Then, aIf.Else);
                    }
                    //}
                    //} else if (aIf.Else == null && bIf.Else == null) {
                    //    // Look for common 'if' condition parts and join them, unless bodies contain continuations
                    //    Func<Expr, IEnumerable<Expr>> getAnds = null;
                    //    getAnds = e => {
                    //        if (e.ExprType == Expr.NodeType.Binary) {
                    //            var eBin = (ExprBinary)e;
                    //            if (eBin.Op == BinaryOp.And) {
                    //                return Enumerable.Concat(getAnds(eBin.Left), getAnds(eBin.Right));
                    //            }
                    //        }
                    //        return new[] { e };
                    //    };
                    //    var aAnds = getAnds(aIf.Condition).ToArray();
                    //    var bAnds = getAnds(bIf.Condition).ToArray();
                    //    var intersectionAnds = aAnds.Intersect(bAnds).ToArray();
                    //    if (intersectionAnds.Any()) {
                    //        var aAndsNoCommon = aAnds.Except(intersectionAnds);
                    //        var bAndsNoCommon = bAnds.Except(intersectionAnds);
                    //        var condition = intersectionAnds.Aggregate((x, y) => this.exprGen.And(x, y));
                    //        var aCond = aAndsNoCommon.Aggregate((Expr)new ExprLiteral(true, this.typeSystem.Boolean), (x, y) => this.exprGen.And(x, y));
                    //        var bCond = bAndsNoCommon.Aggregate((Expr)new ExprLiteral(true, this.typeSystem.Boolean), (x, y) => this.exprGen.And(x, y));
                    //        return new StmtIf(condition,
                    //            new StmtBlock(new StmtIf(aCond, aIf.Then), new StmtIf(bCond, bIf.Then)));
                    //    }
                }
                return null;
            })
            .ToArray();
            StmtBlock ret;
            if (!Enumerable.SequenceEqual(s.Statements, stNew)) {
                ret = new StmtBlock(stNew);
            } else {
                ret = s;
            }
            if (ret.Statements.Count() == 1) {
                return ret.Statements.First();
            } else {
                return ret;
            }
        }

    }
}
