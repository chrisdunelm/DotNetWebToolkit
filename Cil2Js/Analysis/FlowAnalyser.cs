using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Cil2Js.Utils;
using System.Diagnostics;

namespace Cil2Js.Analysis {
    public static class FlowAnalyser {

        class VisitCounter : AstRecursiveVisitor {

            public static int GetCount(ICode countReferences, ICode root) {
                var v = new VisitCounter(countReferences);
                v.Visit(root);
                return v.Count;
            }

            public VisitCounter(ICode countReferences) {
                this.countReferences = countReferences;
                this.Count = 0;
            }

            private ICode countReferences;

            public int Count { get; private set; }

            protected override ICode VisitContinuation(StmtContinuation s) {
                if (s.To == this.countReferences) {
                    this.Count++;
                }
                return base.VisitContinuation(s);
            }

        }

        class VisitorFindContinuations : AstVisitor {

            public static bool Any(ICode root) {
                var v = new VisitorFindContinuations();
                v.Visit(root);
                return v.Continuations.Any();
            }

            public static IEnumerable<StmtContinuation> Get(ICode root) {
                var v = new VisitorFindContinuations();
                v.Visit(root);
                return v.Continuations;
            }

            private List<StmtContinuation> continuations = new List<StmtContinuation>();
            public IEnumerable<StmtContinuation> Continuations { get { return this.continuations; } }

            protected override ICode VisitContinuation(StmtContinuation s) {
                this.continuations.Add(s);
                return s;
            }

        }

        class VisitorOnlyStatements : AstVisitor {

            public static bool Only(ICode c, params Stmt.NodeType[] stmtTypes) {
                var v = new VisitorOnlyStatements(stmtTypes);
                v.Visit(c);
                return v.IsOnlyRequestedTypes;
            }

            public VisitorOnlyStatements(params Stmt.NodeType[] stmtTypes) {
                this.stmtTypes = stmtTypes;
                this.IsOnlyRequestedTypes = true;
            }

            private Stmt.NodeType[] stmtTypes;

            public bool IsOnlyRequestedTypes { get; private set; }

            protected override ICode VisitStmt(Stmt s) {
                if (!this.IsOnlyRequestedTypes || !this.stmtTypes.Contains(s.StmtType)) {
                    this.IsOnlyRequestedTypes = false;
                    return s;
                }
                return base.VisitStmt(s);
            }

            protected override ICode VisitContinuation(StmtContinuation s) {
                // Don't continue through continuations
                return s;
            }

            protected override ICode VisitExpr(Expr e) {
                // Not interested in expressions
                return e;
            }

        }

        public class VisitorSubstitute : AstRecursiveVisitor {

            public static ICode V(ICode c) {
                var v = new VisitorSubstitute(c);
                var c2 = v.Visit(c);
                return c2;
            }

            public VisitorSubstitute(ICode start) {
                this.start = start;
            }

            private ICode start;

            protected override ICode VisitContinuation(StmtContinuation s) {
                if (!s.LeaveProtectedRegion) {
                    // Must never substitute when leaving protected region.
                    // This would change which statements were inside the try/catch/finally region
                    var count = VisitCounter.GetCount(s.To, this.start);
                    if (count == 1) {
                        return this.Visit(s.To);
                    }
                }
                return base.VisitContinuation(s);
            }

        }

        public class VisitorIfDistribution : AstRecursiveVisitor {

            public static ICode V(MethodDefinition method, ICode c) {
                var v = new VisitorIfDistribution(method);
                return v.Visit(c);
            }

            class IfInfo {
                public Stack<Expr> Conditions = new Stack<Expr>();
                public List<Tuple<StmtContinuation, Expr>> AddToIf = new List<Tuple<StmtContinuation, Expr>>();
            }

            private VisitorIfDistribution(MethodDefinition method) {
                this.method = method;
                this.eGen = Expr.ExprGen(method.Module.TypeSystem);
            }

            private MethodDefinition method;
            private Expr.Gen eGen;
            private IfInfo ifInfo = null;

            protected override ICode VisitIf(StmtIf s) {
                if (!VisitorFindContinuations.Any(s)) {
                    // 'If' contains no continuations, so no distribution can be done
                    return s;
                }
                if (VisitorOnlyStatements.Only(s, Stmt.NodeType.If, Stmt.NodeType.Continuation) && this.ifInfo == null) {
                    // 'If' only contains continuations, so no distribution can be done
                    // Must visit base method to find contained continuations
                    return base.VisitIf(s);
                }
                bool finalise = false;
                if (this.ifInfo == null) {
                    finalise = true;
                    this.ifInfo = new IfInfo();
                }
                this.ifInfo.Conditions.Push(s.Condition);
                var then = this.Visit(s.Then);
                this.ifInfo.Conditions.Pop();
                this.ifInfo.Conditions.Push(this.eGen.NotAutoSimplify(s.Condition));
                var @else = this.Visit(s.Else);
                this.ifInfo.Conditions.Pop();
                if (then != s.Then || @else != s.Else) {
                    var @if = new StmtIf(s.Condition, (Stmt)then, (Stmt)@else);
                    if (finalise && this.ifInfo.AddToIf.Any()) {
                        var ifStmts = this.ifInfo.AddToIf.GroupBy(x => x.Item1.To, x => x.Item2).Select(x =>
                            new StmtIf(x.Aggregate((a, b) => this.eGen.Or(a, b)),
                                this.ifInfo.AddToIf.First(y => y.Item1.To == x.Key).Item1)
                            );
                        var stmts = new Stmt[] { @if }.Concat(ifStmts).ToArray();
                        this.ifInfo = null;
                        return new StmtBlock(stmts);
                    } else {
                        if (finalise) {
                            this.ifInfo = null;
                        }
                        return @if;
                    }
                } else {
                    // In this case, no continuations will have been found, so there cannot be any conditions to add
                    if (finalise) {
                        this.ifInfo = null;
                    }
                    return s;
                }
            }

            protected override ICode VisitContinuation(StmtContinuation s) {
                if (this.ifInfo == null) {
                    // If there are no if statements in this continuation
                    return base.VisitContinuation(s);
                }
                if (!this.ifInfo.Conditions.Any()) {
                    throw new InvalidOperationException("There should be one or more conditions at this point");
                }
                var combinedCondition = this.ifInfo.Conditions.Aggregate((a, b) => this.eGen.And(a, b));
                this.ifInfo.AddToIf.Add(Tuple.Create(s, combinedCondition));
                base.VisitContinuation(s);
                return null;
            }

            protected override ICode VisitDoLoop(StmtDoLoop s) {
                return this.Isolate(() => base.VisitDoLoop(s));
            }

            protected override ICode VisitTry(StmtTry s) {
                return this.Isolate(() => base.VisitTry(s));
            }

            private ICode Isolate(Func<ICode> fn) {
                var stack = this.ifInfo;
                this.ifInfo = null;
                var ret = fn();
                this.ifInfo = stack;
                return ret;
            }

        }

        public class VisitorDerecurse : AstRecursiveVisitor {

            public static ICode V(ICode c) {
                var v = new VisitorDerecurse();
                return v.Visit(c);
            }

            private VisitorDerecurse() { }

            private Dictionary<ICode, ICode> replaces = new Dictionary<ICode, ICode>();

            protected override ICode VisitStmt(Stmt s) {
                var r = this.replaces.ValueOrDefault(s);
                if (r != null) {
                    this.map.Add(s, r);
                    return r;
                }
                return base.VisitStmt(s);
            }

            protected override ICode VisitContinuation(StmtContinuation s) {
                if (s.To.StmtType != Stmt.NodeType.Block) {
                    return base.VisitContinuation(s);
                }
                var block = (StmtBlock)s.To;
                foreach (var stmt in block.Statements) {
                    if (stmt.StmtType == Stmt.NodeType.If) {
                        var sIf = (StmtIf)stmt;
                        if (sIf.Else == null && sIf.Then.StmtType == Stmt.NodeType.Continuation) {
                            var sThen = (StmtContinuation)sIf.Then;
                            if (sThen.To == s.To) {
                                // Recursive, so derecurse
                                var condition = sIf.Condition;
                                var bodyStmts = block.Statements.TakeWhile(x => x != stmt).ToArray();
                                var bodyLast = bodyStmts.LastOrDefault();
                                var body = new StmtBlock(bodyStmts);
                                var loop = new StmtDoLoop(body, condition);
                                var afterLoop = block.Statements.SkipWhile(x => x != stmt).Skip(1).ToArray();
                                Stmt replaceWith;
                                if (afterLoop.Any()) {
                                    var loopAndAfter = new[] { loop }.Concat(afterLoop).ToArray();
                                    replaceWith = new StmtBlock(loopAndAfter);
                                } else {
                                    replaceWith = loop;
                                }
                                this.replaces.Add(s.To, replaceWith);
                                return base.VisitContinuation(s);
                            }
                        }
                    }
                    if (VisitorFindContinuations.Any(stmt)) {
                        // Another continuation present, cannot derecurse
                        break;
                    }
                }
                return base.VisitContinuation(s);
            }

        }

        public class VisitorMoveOutOfLoop : AstVisitor {

            public static ICode V(ICode c) {
                var v = new VisitorMoveOutOfLoop();
                return v.Visit(c);
            }

            private VisitorMoveOutOfLoop() { }

            protected override ICode VisitDoLoop(StmtDoLoop s) {
                var body = (Stmt)this.Visit(s.Body);
                StmtIf lastIf = null;
                IEnumerable<Stmt> preIf = null;
                if (body.StmtType == Stmt.NodeType.Block) {
                    var sBlock = (StmtBlock)body;
                    if (sBlock.Statements.Any()) {
                        var sLast = sBlock.Statements.Last();
                        if (sLast.StmtType == Stmt.NodeType.If) {
                            lastIf = (StmtIf)sLast;
                            preIf = sBlock.Statements.Take(sBlock.Statements.Count() - 1).ToArray();
                        }
                    }
                } else if (body.StmtType == Stmt.NodeType.If) {
                    lastIf = (StmtIf)body;
                    preIf = Enumerable.Empty<Stmt>();
                }
                if (lastIf != null) {
                    Stmt afterLoop = null;
                    StmtIf newIf = null;
                    // See if final 'if' condition is same as the 'do' condition.
                    // TODO: This may lead to a non-terminating situation...
                    if (lastIf.Condition.DoesEqual(s.While)) {
                        afterLoop = lastIf.Else;
                        newIf = new StmtIf(lastIf.Condition, lastIf.Then, null);
                    } else if (lastIf.Condition.DoesEqualNot(s.While)) {
                        afterLoop = lastIf.Then;
                        newIf = new StmtIf(lastIf.Condition, null, lastIf.Else);
                    }
                    if (afterLoop != null) {
                        var loopBody = new StmtBlock(preIf.Concat(newIf));
                        var loop = new StmtDoLoop(loopBody, s.While);
                        var ret = new StmtBlock(loop, afterLoop);
                        return ret;
                    }
                }
                if (body != s.Body) {
                    return new StmtDoLoop(body, s.While);
                } else {
                    return s;
                }
            }

        }

        class VisitorEmptyBlockRemoval : AstRecursiveVisitor {

            public static ICode V(ICode c) {
                var v = new VisitorEmptyBlockRemoval();
                return v.Visit(c);
            }

            protected override ICode VisitBlock(StmtBlock s) {
                var count = s.Statements.Count();
                if (count == 0) {
                    return null;
                }
                if (count == 1) {
                    return this.Visit(s.Statements.First());
                }
                return base.VisitBlock(s);
            }

        }

        class VisitorTryCatchFinallySequencing : AstRecursiveVisitor {

            public static ICode V(ICode ast) {
                var v = new VisitorTryCatchFinallySequencing();
                return v.Visit(ast);
            }

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
                        return Tuple.Create((Stmt)new StmtEmpty(), cont.To);
                    }
                    return Tuple.Create((Stmt)new StmtBlock(statements.Take(statements.Length - 1)), cont.To);
                case Stmt.NodeType.Continuation:
                    var sCont = (StmtContinuation)s;
                    if (!sCont.LeaveProtectedRegion) {
                        return null;
                    }
                    return Tuple.Create((Stmt)new StmtEmpty(), sCont.To);
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
                            var newTry = new StmtTry(@try.Item1, new[] { new StmtTry.Catch(@catch.Item1, sCatch.ExceptionObject) }, null);
                            return new StmtBlock(newTry, @try.Item2 ?? @catch.Item2);
                        }
                    }
                    if (s.Finally != null) {
                        var @finally = this.RemoveContinuation(s.Finally);
                        if ((@try.Item2 == null || @finally.Item2 == null || @try.Item2 == @finally.Item2) && (@try.Item2 != null || @finally.Item2 != null)) {
                            var newTry = new StmtTry(@try.Item1, null, @finally.Item1);
                            return new StmtBlock(newTry, @try.Item2 ?? @finally.Item2);
                        }
                    }
                }
                return base.VisitTry(s);
            }

        }

        public static Stmt Analyse(MethodDefinition method, Stmt stmt0, bool verbose = false) {
            int step = 0;
            Action<Stmt, string> print = (s, name) => {
                if (verbose) {
                    if (name != null && name.StartsWith("Visitor")) {
                        name = name.Substring(7);
                    }
                    Console.WriteLine(" --- AST Transform Step {0}{1} ---", step++, name == null ? "" : (" '" + name + "'"));
                    Console.WriteLine(ShowVisitor.V(method, s));
                    Console.WriteLine();
                }
            };
            Func<Func<Stmt, Stmt>, Stmt, string, Stmt> doStep = (fn, s0, name) => {
                var s1 = fn(s0);
                if (s1 != s0) {
                    print(s1, name);
                }
                return s1;
            };
            print(stmt0, null);
            var stmt = doStep(s => (Stmt)VisitorConvertCilToSsa.V(method, s), stmt0, "VisitorConvertCilToSsa");
            // Reduce to AST with no continuations
            for (int i = 0; ; i++) {
                var stmtOrg = stmt;
                stmt = doStep(s => (Stmt)VisitorSubstitute.V(s), stmt, "VisitorSubstitute");
                stmt = doStep(s => (Stmt)VisitorTryCatchFinallySequencing.V(s), stmt, "VisitorTryCatchFinallySequencing");
                stmt = doStep(s => (Stmt)VisitorIfDistribution.V(method, s), stmt, "VisitorIfDistribution");
                stmt = doStep(s => (Stmt)VisitorDerecurse.V(s), stmt, "VisitorDerecurse");
                stmt = doStep(s => (Stmt)VisitorBooleanSimplification.V(method, s), stmt, "VisitorBooleanSimplification");
                stmt = doStep(s => (Stmt)VisitorIfSimplification.V(method, s), stmt, "VisitorIfSimplification");
                stmt = doStep(s => (Stmt)VisitorConditionRemoval.V(method, s), stmt, "VisitorConditionRemoval");
                stmt = doStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), stmt, "VisitorEmptyBlockRemoval");
                if (stmt == stmtOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to reduce AST");
                }
            }
            if (VisitorFindContinuations.Any(stmt)) {
                throw new InvalidOperationException("Error: Cannot reduce IL to AST with no continuations");
            }
            // Simplify AST
            for (int i = 0; ; i++) {
                var stmtOrg = stmt;
                stmt = doStep(s => (Stmt)VisitorBooleanSimplification.V(method, s), stmt, "VisitorBooleanSimplification");
                stmt = doStep(s => (Stmt)VisitorIfSimplification.V(method, s), stmt, "VisitorIfSimplification");
                stmt = doStep(s => (Stmt)VisitorMoveOutOfLoop.V(s), stmt, "VisitorMoveOutOfLoop");
                stmt = doStep(s => (Stmt)VisitorConditionRemoval.V(method, s), stmt, "VisitorConditionRemoval");
                stmt = doStep(s => (Stmt)VisitorSsaSimplifier.V(s), stmt, "VisitorSsaSimplifier");
                stmt = doStep(s => (Stmt)VisitorPhiSimplifier.V(s), stmt, "VisitorPhiSimplifier");
                stmt = doStep(s => (Stmt)VisitorExpressionSimplifier.V(method, s), stmt, "VisitorExpressionSimplifier");
                stmt = doStep(s => (Stmt)VisitorEmptyBlockRemoval.V(s), stmt, "VisitorEmptyBlockRemoval");
                if (stmt == stmtOrg) {
                    break;
                }
                if (i > 20) {
                    // After 20 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to simplify AST");
                }
            }
            stmt = doStep(s => (Stmt)VisitorRemoveCasts.V(method.Module.TypeSystem, s), stmt, "VisitorRemoveCasts");
            return stmt;
        }

    }
}
