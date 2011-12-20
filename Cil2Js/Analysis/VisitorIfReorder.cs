using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.Analysis {
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
            var statements = s.Statements
                .Select(x => (Stmt)this.Visit(x))
                .Where(x => x != null)
                .ToArray();
            if (statements.Any()) {
                var last = statements.Last();
                if (last.StmtType == Stmt.NodeType.Continuation) {
                    // If the last statement is a continuation and recurses, and there is a previous 'if' statement
                    // with a continuation, then swap them
                    var lastCont = (StmtContinuation)last;
                    if (lastCont.To == this.block) {
                        var splitIf = (StmtIf)statements.FirstOrDefault(x => {
                            if (x.StmtType == Stmt.NodeType.If) {
                                var xIf = (StmtIf)x;
                                if (xIf.Else == null && xIf.Then != null && xIf.Then.StmtType == Stmt.NodeType.Continuation) {
                                    return true;
                                }
                            }
                            return false;
                        });
                        if (splitIf != null) {
                            var beforeIf = statements.TakeWhile(x => x != splitIf).ToArray();
                            var afterIf = statements.SkipWhile(x => x != splitIf).Skip(1).ToArray();
                            var newIf = new StmtIf(s.Ctx, s.Ctx.ExprGen.NotAutoSimplify(splitIf.Condition), new StmtBlock(s.Ctx, afterIf), null);
                            var allStmts = beforeIf.Concat(newIf).Concat(splitIf.Then).ToArray();
                            return new StmtBlock(s.Ctx, allStmts);
                        }
                    }
                }
            }
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
                            return new StmtBlock(s.Ctx, b, a);
                        }
                    }
                }
                // If an 'if' statement containing only a continuation is followed by any other kind of statement then swap them
                // (with suitable 'if' guard)
                if (a.StmtType == Stmt.NodeType.If) {
                    var aIf = (StmtIf)a;
                    if (aIf.Then != null && aIf.Then.StmtType == Stmt.NodeType.Continuation && aIf.Else == null) {
                        bool swap;
                        if (b.StmtType != Stmt.NodeType.If) {
                            swap = true;
                        } else {
                            var bIf = (StmtIf)b;
                            swap = bIf.Then == null || bIf.Then.StmtType != Stmt.NodeType.Continuation || bIf.Else != null;
                        }
                        if (swap) {
                            return new StmtBlock(s.Ctx,
                                new StmtIf(s.Ctx, s.Ctx.ExprGen.NotAutoSimplify(aIf.Condition), b, null),
                                a);
                        }
                    }
                }
                return null;
            })
            .ToArray();
            if (!Enumerable.SequenceEqual(s.Statements, stNew)) {
                return new StmtBlock(s.Ctx, stNew);
            } else {
                return s;
            }
        }

    }
}
