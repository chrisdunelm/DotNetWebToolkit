using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorIfReorder : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorIfReorder();
            return v.Visit(ast);
        }

        private ICode block;

        protected override void BlockStart(ICode block) {
            this.block = block;
        }

        protected override ICode VisitBlock(StmtBlock s) {
            var ctx = s.Ctx;
            var statements = s.Statements
                .Select(x => (Stmt)this.Visit(x))
                .Where(x => x != null)
                .ToArray();
            var stNew = statements.Combine((a, b) => {
                // If two 'if' statements are both continuations, then bring a recursive continuation forwards if possible
                if (a.StmtType == Stmt.NodeType.If && b.StmtType == Stmt.NodeType.If) {
                    var aIf = (StmtIf)a;
                    var bIf = (StmtIf)b;
                    if (aIf.Then != null && bIf.Then != null
                        && aIf.Then.StmtType == Stmt.NodeType.Continuation && bIf.Then.StmtType == Stmt.NodeType.Continuation
                        && aIf.Else == null && bIf.Else == null) {
                        var aCont = (StmtContinuation)aIf.Then;
                        var bCont = (StmtContinuation)bIf.Then;
                        if (aCont.To != this.block && bCont.To == this.block) {
                            return new StmtBlock(ctx, b, a);
                        }
                    }
                }
                //// If an 'if' statement containing only a continuation is followed by any other kind of statement then swap them
                //// (with suitable 'if' guard). Look ahead and encase as much as possible in the 'if' guard
                //if (a.StmtType == Stmt.NodeType.If) {
                //    var aIf = (StmtIf)a;
                //    if (aIf.Then != null && aIf.Then.StmtType == Stmt.NodeType.Continuation && aIf.Else == null) {
                //        bool swap;
                //        if (b.StmtType != Stmt.NodeType.If) {
                //            swap = true;
                //        } else {
                //            var bIf = (StmtIf)b;
                //            swap = bIf.Then == null || bIf.Then.StmtType != Stmt.NodeType.Continuation || bIf.Else != null;
                //        }
                //        if (swap) {
                //            return new StmtBlock(s.Ctx,
                //                new StmtIf(s.Ctx, s.Ctx.ExprGen.NotAutoSimplify(aIf.Condition), b, null),
                //                a);
                //        }
                //    }
                //}
                return null;
            })
            .ToArray();
            if (!statements.SequenceEqual(stNew)) {
                return new StmtBlock(ctx, stNew);
            }
            // If an 'if' statement containing only a continuation is followed by any other kind of statement then swap them
            // (with suitable 'if' guard). Look ahead and encase as much as possible in the 'if' guard
            for (int i = 0; i < statements.Length - 1; i++) {
                var stmt = statements[i];
                if (stmt.StmtType == Stmt.NodeType.If) {
                    var sIf = (StmtIf)stmt;
                    if (sIf.Then != null && sIf.Then.StmtType == Stmt.NodeType.Continuation && sIf.Else == null) {
                        var moveCount = 0;
                        for (int j = i + 1; j < statements.Length; j++) {
                            var b = statements[j];
                            bool move;
                            if (b.StmtType == Stmt.NodeType.Continuation) {
                                move = false;
                            } else if (b.StmtType != Stmt.NodeType.If) {
                                move = true;
                            } else {
                                var bIf = (StmtIf)b;
                                move = bIf.Then == null || bIf.Then.StmtType != Stmt.NodeType.Continuation || bIf.Else != null;
                            }
                            if (move) {
                                moveCount++;
                            } else {
                                break;
                            }
                        }
                        if (moveCount > 0) {
                            var moveBlock = new StmtBlock(ctx, statements.Skip(i + 1).Take(moveCount));
                            var ifBlock = new StmtIf(ctx, ctx.ExprGen.Not(sIf.Condition), moveBlock, null);
                            var allStmts =
                                statements.Take(i)
                                .Concat(ifBlock)
                                .Concat(statements[i])
                                .Concat(statements.Skip(i + 1 + moveCount))
                                .ToArray();
                            return new StmtBlock(ctx, allStmts);
                        }
                    }
                }
            }

            // Reorder if/continuations at end of block to group by continuation target
            var finalContsRev = statements.Reverse().TakeWhile(x => {
                if (x.StmtType == Stmt.NodeType.Continuation) {
                    return true;
                }
                if (x.StmtType == Stmt.NodeType.If) {
                    var xIf = (StmtIf)x;
                    if (xIf.Else == null && xIf.Then.StmtType == Stmt.NodeType.Continuation) {
                        return true;
                    }
                }
                return false;
            })
            .Select(x => {
                Stmt to;
                if (x.StmtType == Stmt.NodeType.Continuation) {
                    to = ((StmtContinuation)x).To;
                } else {
                    to = ((StmtContinuation)((StmtIf)x).Then).To;
                }
                return new { stmt = x, to };
            }).ToArray();
            var lookForMidCont = finalContsRev.Reverse().TakeWhile(x => x.stmt.StmtType != Stmt.NodeType.Continuation).ToArray();
            if (lookForMidCont.Length < finalContsRev.Length - 1) {
                // Look for non-if continuation in middle of final continuations
                // I suspect that this cannot ever occur
                var stmts = statements.Take(statements.Length - finalContsRev.Length)
                    .Concat(lookForMidCont.Select(x => x.stmt))
                    .Concat(finalContsRev.Select(x => x.stmt).Reverse().ElementAt(lookForMidCont.Length)).ToArray();
                return new StmtBlock(s.Ctx, stmts);
            }
            if (finalContsRev.Count() >= 2) {
                var newFinal = finalContsRev.Combine((a, b) => {
                    if (a.stmt.StmtType == Stmt.NodeType.Continuation && a.to == b.to) {
                        return a;
                    }
                    return null;
                });
                if (!finalContsRev.SequenceEqual(newFinal)) {
                    var stmts = statements.Take(statements.Length - finalContsRev.Length).Concat(newFinal.Reverse().Select(x => x.stmt));
                    return new StmtBlock(s.Ctx, stmts);
                }
                var dups = finalContsRev.GroupBy(x => x.to).Select(x => new { to = x.Key, count = x.Count() }).Where(x => x.count >= 2).ToArray();
                if (dups.Any()) {
                    var finalStmts = finalContsRev.Reverse().ToArray();
                    foreach (var dup in dups) {
                        var movingBack = new List<Stmt>();
                        var addIfs = new Dictionary<Stmt, IEnumerable<Expr>>();
                        foreach (var stmt in finalStmts) {
                            if (stmt.to == dup.to) {
                                movingBack.Add(stmt.stmt);
                            } else {
                                addIfs.Add(stmt.stmt, movingBack.Select(x => s.Ctx.ExprGen.Not(((StmtIf)x).Condition)).ToArray());
                            }
                            if (movingBack.Count == dup.count) {
                                finalStmts = finalStmts.SelectMany(x => {
                                    if (movingBack.Contains(x.stmt)) {
                                        if (x.stmt == movingBack.Last()) {
                                            return movingBack.Select(y => new { stmt = (Stmt)y, to = x.to });
                                        } else {
                                            return finalStmts.EmptyOf();
                                        }
                                    } else {
                                        var addIf = addIfs.ValueOrDefault(x.stmt);
                                        if (addIf != null && addIf.Any()) {
                                            var @if = addIf.Aggregate((a, b) => s.Ctx.ExprGen.And(a, b));
                                            var newIf = new StmtIf(s.Ctx, @if, x.stmt, null);
                                            return new[] { new { stmt = (Stmt)newIf, to = x.to } };
                                        } else {
                                            return new[] { x };
                                        }
                                    }
                                }).ToArray();

                                break;
                            }
                        }
                    }
                    var stmts = statements.Take(statements.Length - finalContsRev.Length).Concat(finalStmts.Select(x => x.stmt));
                    return new StmtBlock(s.Ctx, stmts);
                }
            }
            return s;
        }

    }
}
