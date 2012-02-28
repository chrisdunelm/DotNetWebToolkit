using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorTryCatchFinallySequencing : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorTryCatchFinallySequencing();
            return v.Visit(ast);
        }

        private VisitorTryCatchFinallySequencing() { }

        private Tuple<Stmt, Stmt> RemoveContinuation(Stmt s) {
            // This must not return a null statement if empty, as then the 'try' statements won't know
            // if it is a 'catch' or 'finally' statement. Uses a StmtEmpty instead.
            var contCount = VisitorFindContinuations.Get(s).Count();
            if (contCount == 0) {
                // Blocks with no continuations must end with a 'throw'
                return Tuple.Create(s, (Stmt)null);
            }
            if (contCount != 1) {
                return null;
            }
            switch (s.StmtType) {
            case Stmt.NodeType.Block:
                var statements = ((StmtBlock)s).Statements.ToArray();
                if (statements.Length == 0) {
                    return null;
                }
                if (statements.Last().StmtType != Stmt.NodeType.Continuation) {
                    return null;
                }
                var cont = (StmtContinuation)statements.Last();
                if (!cont.LeaveProtectedRegion) {
                    return null;
                }
                if (statements.Length == 1) {
                    return Tuple.Create((Stmt)new StmtEmpty(s.Ctx), cont.To);
                }
                return Tuple.Create((Stmt)new StmtBlock(s.Ctx, statements.Take(statements.Length - 1)), cont.To);
            case Stmt.NodeType.Continuation:
                var sCont = (StmtContinuation)s;
                if (!sCont.LeaveProtectedRegion) {
                    return null;
                }
                return Tuple.Create((Stmt)new StmtEmpty(s.Ctx), sCont.To);
            default:
                return null;
            }
        }

        protected override ICode VisitTry(StmtTry s) {
            var @try = this.RemoveContinuation(s.Try);
            if (@try != null) {
                if (s.Catches != null) {
                    if (s.Catches.Count() != 1) {
                        throw new InvalidOperationException("Should only ever see 1 catch here");
                    }
                    var sCatch = s.Catches.First();
                    var @catch = this.RemoveContinuation(sCatch.Stmt);
                    if ((@try.Item2 == null || @catch.Item2 == null || @try.Item2 == @catch.Item2) && (@try.Item2 != null || @catch.Item2 != null)) {
                        var newTry = new StmtTry(s.Ctx, @try.Item1, new[] { new StmtTry.Catch(@catch.Item1, sCatch.ExceptionVar) }, null);
                        return new StmtBlock(s.Ctx, newTry, @try.Item2 ?? @catch.Item2);
                    }
                    // Special case
                    // When 'leave' CIL branch to different instructions, allow specific code to be
                    // moved inside the 'try' or 'catch' block. It should be impossible for this code to throw an exception
                    var tryTos = VisitorFindContinuations.Get(@try.Item2);
                    if (tryTos.Count() == 1 && tryTos.First().To == @catch.Item2 && @try.Item2.StmtType == Stmt.NodeType.Block) {
                        var try2Stmts = ((StmtBlock)@try.Item2).Statements.ToArray();
                        var s0 = try2Stmts.Take(try2Stmts.Length - 1);
                        if (s0.All(x => x.StmtType == Stmt.NodeType.Assignment)) {
                            var sN = try2Stmts.Last();
                            if (sN.StmtType == Stmt.NodeType.Continuation) {
                                var newTry = new StmtTry(s.Ctx,
                                    new StmtBlock(s.Ctx, @try.Item1, new StmtBlock(s.Ctx, s0), new StmtContinuation(s.Ctx, ((StmtContinuation)sN).To, true)),
                                    s.Catches, null);
                                return newTry;
                            }
                        }
                    }
                }
                if (s.Finally != null) {
                    var @finally = this.RemoveContinuation(s.Finally);
                    if ((@try.Item2 == null || @finally.Item2 == null || @try.Item2 == @finally.Item2) && (@try.Item2 != null || @finally.Item2 != null)) {
                        var newTry = new StmtTry(s.Ctx, @try.Item1, null, @finally.Item1);
                        return new StmtBlock(s.Ctx, newTry, @try.Item2 ?? @finally.Item2);
                    }
                }
                // TODO: This is a hack for badly handling fault handlers. They are ignored at the moment
                if (s.Catches == null && s.Finally == null) {
                    return new StmtBlock(s.Ctx, @try.Item1, new StmtContinuation(s.Ctx, @try.Item2, false));
                }
            }
            return base.VisitTry(s);
        }

    }
}
