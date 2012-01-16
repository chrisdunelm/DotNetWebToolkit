using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorSwitchSequencing : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorSwitchSequencing();
            return v.Visit(ast);
        }

        protected override ICode VisitSwitch(StmtSwitch s) {
            // If switch statement contains no continuations then it doesn't need processing
            if (!VisitorFindContinuations.Any(s)) {
                return base.VisitSwitch(s);
            }
            // If any cases go to the same continuation as the default case, remove them
            if (s.Default != null && s.Default.StmtType == Stmt.NodeType.Continuation) {
                var defaultCont = (StmtContinuation)s.Default;
                var sameAsDefault = s.Cases
                    .Where(x => x.Stmt != null && x.Stmt.StmtType == Stmt.NodeType.Continuation && ((StmtContinuation)x.Stmt).To == defaultCont.To)
                    .ToArray();
                if (sameAsDefault.Any()) {
                    var cases = s.Cases.Except(sameAsDefault);
                    return new StmtSwitch(s.Ctx, s.Expr, cases, s.Default);
                }
            }
            // If multiple case statements all go the same continuation, then put them consequetively
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
                return new StmtSwitch(s.Ctx, s.Expr, allCases, s.Default);
            }
            // If all cases end with a continuation to the same stmt, then put that stmt after the switch and remove all continuations
            var conts = s.Cases.Select(x => x.Stmt).Concat(s.Default).SelectMany(x => {
                if (x == null) {
                    return Enumerable.Empty<StmtContinuation>();
                }
                var contCount = VisitorFindContinuations.Get(x).Count();
                if (contCount == 0) {
                    // Case contains return or throw
                    return Enumerable.Empty<StmtContinuation>();
                }
                if (contCount == 1) {
                    if (x.StmtType == Stmt.NodeType.Continuation) {
                        return new[] { (StmtContinuation)x };
                    }
                    if (x.StmtType == Stmt.NodeType.Block) {
                        var xBlock = (StmtBlock)x;
                        var last = xBlock.Statements.Last();
                        if (last.StmtType == Stmt.NodeType.Continuation) {
                            return new[] { (StmtContinuation)last };
                        }
                    }
                }
                return new StmtContinuation[] { null };
            }).ToArray();
            if (conts.All(x => x != null) && conts.AllSame(x => x.To)) {
                Func<Stmt, Stmt> removeCont = stmt => {
                    if (stmt == null) {
                        return null;
                    }
                    switch (stmt.StmtType) {
                    case Stmt.NodeType.Continuation:
                        return new StmtBreak(s.Ctx);
                    case Stmt.NodeType.Block:
                        var sBlock = (StmtBlock)stmt;
                        var stmts = sBlock.Statements.ToArray();
                        if (stmts.Last().StmtType == Stmt.NodeType.Continuation) {
                            stmts = stmts.Take(stmts.Length - 1).Concat(new StmtBreak(s.Ctx)).ToArray();
                            return new StmtBlock(s.Ctx, stmts);
                        } else {
                            return stmt;
                        }
                    default:
                        return stmt;
                    }
                };
                var cases = s.Cases.Select(x => new StmtSwitch.Case(x.Value, removeCont(x.Stmt))).ToArray();
                var @switch = new StmtSwitch(s.Ctx, s.Expr, cases, removeCont(s.Default));
                return new StmtBlock(s.Ctx, @switch, conts[0]);
            }

            return base.VisitSwitch(s);
        }

    }
}
