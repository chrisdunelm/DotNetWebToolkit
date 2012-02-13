using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorSwitchSequencing : AstRecursiveVisitor {

        public static ICode V(ICode ast, bool lastChance) {
            var v = new VisitorSwitchSequencing(lastChance);
            return v.Visit(ast);
        }

        private VisitorSwitchSequencing(bool lastChance) {
            this.lastChance = lastChance;
        }

        private bool lastChance;

        protected override ICode VisitSwitch(StmtSwitch s) {
            // If switch statement contains no continuations then it doesn't need processing
            if (!VisitorFindContinuations.Any(s)) {
                return base.VisitSwitch(s);
            }
            var ctx = s.Ctx;
            // If any cases go to the same continuation as the default case, remove them
            if (s.Default != null && s.Default.StmtType == Stmt.NodeType.Continuation) {
                var defaultCont = (StmtContinuation)s.Default;
                var sameAsDefault = s.Cases
                    .Where(x => x.Stmt != null && x.Stmt.StmtType == Stmt.NodeType.Continuation && ((StmtContinuation)x.Stmt).To == defaultCont.To)
                    .ToArray();
                if (sameAsDefault.Any()) {
                    var cases = s.Cases.Except(sameAsDefault);
                    return new StmtSwitch(ctx, s.Expr, cases, s.Default);
                }
            }
            // If multiple case statements all go the same continuation, then put them consecutively
            var groupedByTo = s.Cases
                .Where(x => x.Stmt != null && x.Stmt.StmtType == Stmt.NodeType.Continuation)
                .GroupBy(x => ((StmtContinuation)x.Stmt).To)
                .Where(x => x.Count() >= 2)
                .ToArray();
            if (groupedByTo.Any()) {
                var cases = s.Cases.Except(groupedByTo.SelectMany(x => x));
                var combinedCases = groupedByTo.SelectMany(x => {
                    var same = x.ToArray();
                    var last = same.Last();
                    var sameCases = same.Take(same.Length - 1).Select(y => new StmtSwitch.Case(y.Value, null))
                        .Concat(new StmtSwitch.Case(last.Value, last.Stmt));
                    return sameCases;
                });
                var allCases = cases.Concat(combinedCases).ToArray();
                return new StmtSwitch(ctx, s.Expr, allCases, s.Default);
            }
            Func<Stmt, IEnumerable<StmtContinuation>> getSingleFinalContinuation = stmt => {
                if (stmt == null) {
                    return Enumerable.Empty<StmtContinuation>();
                }
                var contCount = VisitorFindContinuations.Get(stmt).Count();
                if (contCount == 0) {
                    // Case contains return or throw
                    return Enumerable.Empty<StmtContinuation>();
                }
                if (contCount == 1) {
                    if (stmt.StmtType == Stmt.NodeType.Continuation) {
                        return new[] { (StmtContinuation)stmt };
                    }
                    if (stmt.StmtType == Stmt.NodeType.Block) {
                        var stmtBlock = (StmtBlock)stmt;
                        var last = stmtBlock.Statements.LastOrDefault();
                        if (last != null && last.StmtType == Stmt.NodeType.Continuation) {
                            return new[] { (StmtContinuation)last };
                        }
                    }
                }
                return new StmtContinuation[] { null };
            };
            var conts = s.Cases.Select(x => x.Stmt).Concat(s.Default).SelectMany(x => getSingleFinalContinuation(x)).ToArray();
            if (conts.All(x => x != null)) {
                // If all cases end with a continuation to the same stmt, then put that stmt after the switch and remove all continuations
                if (conts.AllSame(x => x.To)) {
                    Func<Stmt, Stmt> removeCont = stmt => {
                        if (stmt == null) {
                            return null;
                        }
                        switch (stmt.StmtType) {
                        case Stmt.NodeType.Continuation:
                            return new StmtBreak(ctx);
                        case Stmt.NodeType.Block:
                            var sBlock = (StmtBlock)stmt;
                            var stmts = sBlock.Statements.ToArray();
                            if (stmts.Last().StmtType == Stmt.NodeType.Continuation) {
                                stmts = stmts.Take(stmts.Length - 1).Concat(new StmtBreak(ctx)).ToArray();
                                return new StmtBlock(ctx, stmts);
                            } else {
                                return stmt;
                            }
                        default:
                            return stmt;
                        }
                    };
                    var cases = s.Cases.Select(x => new StmtSwitch.Case(x.Value, removeCont(x.Stmt))).ToArray();
                    var @switch = new StmtSwitch(ctx, s.Expr, cases, removeCont(s.Default));
                    return new StmtBlock(ctx, @switch, conts[0]);
                } else if (this.lastChance) {
                    // HACK: Change it into multiple if statements
                    var multiValues = new List<int>();
                    var converted = s.Cases.Aggregate(s.Default, (@else, @case) => {
                        multiValues.Add(@case.Value);
                        if (@case.Stmt == null) {
                            return @else;
                        } else {
                            var cond = multiValues.Aggregate((Expr)ctx.Literal(false), (expr, caseValue) => {
                                return ctx.ExprGen.Or(expr, ctx.ExprGen.Equal(s.Expr, ctx.Literal(caseValue)));
                            });
                            multiValues.Clear();
                            var @if = new StmtIf(ctx, cond, @case.Stmt, @else);
                            return @if;
                        }
                    });
                    return converted;
                }
                // If some cases end in a continuation that itself ends in a continuation that other cases end with
                // then use an extra variable to store whether to execute the intermediate code
                // TODO: This is too specific, need a more general-purpose solution to the problem where cases
                // don't all end by going to the same place
                //var contTos = conts.Select(x => x.To).Distinct().ToArray();
                //var finalContTos = contTos.Select(x => getSingleFinalContinuation(x).Select(y => y.NullThru(z => z.To))).SelectMany(x => x).ToArray();
                //if (!finalContTos.Any(x => x == null)) {
                //    // All continuations are fully substituted
                //    var distinctFinalContTos = finalContTos.Distinct().ToArray();
                //    if (distinctFinalContTos.Length == 1) {
                //        var selector = ctx.Local(ctx.Int32);
                //        var inIfCont = contTos.Single(x => x != distinctFinalContTos[0]);
                //        var inIf = new StmtContinuation(ctx, inIfCont, false);
                //        var afterIf = new StmtContinuation(ctx, distinctFinalContTos[0], false);
                //        var allCasesTo = new StmtBlock(ctx,
                //            new StmtIf(ctx, ctx.ExprGen.Equal(selector, ctx.Literal(1)), inIf, null),
                //            afterIf);
                //        Func<Stmt, Stmt> adjustCont = stmt => {
                //            var cont = VisitorFindContinuations.Get(stmt).Single();
                //            var newCont = new StmtContinuation(ctx, allCasesTo, false);
                //            var contChanged = (Stmt)VisitorReplace.V(stmt, cont, newCont);
                //            var sValue = cont.To == inIf.To ? 1 : 0;
                //            var withSelectorSet = new StmtBlock(ctx,
                //                new StmtAssignment(ctx, selector, ctx.Literal(sValue)),
                //                contChanged);
                //            return withSelectorSet;
                //        };
                //        var cases = s.Cases.Select(x => new StmtSwitch.Case(x.Value, adjustCont(x.Stmt))).ToArray();
                //        var @switch = new StmtSwitch(ctx, s.Expr, cases, adjustCont(s.Default));
                //        return @switch;
                //    } else {
                //        throw new NotImplementedException();
                //    }
                //}
            }

            return base.VisitSwitch(s);
        }

    }
}
