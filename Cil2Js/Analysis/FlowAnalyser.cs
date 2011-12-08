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
                var count = VisitCounter.GetCount(s.To, this.start);
                if (count == 1) {
                    return this.Visit(s.To);
                } else {
                    return base.VisitContinuation(s);
                }
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
                if (!s.Statements.Any()) {
                    return null;
                }
                return base.VisitBlock(s);
            }

        }

        public static Stmt Analyse(MethodDefinition method, Stmt stmt0, bool verbose = false) {
            int step = 0;
            Action<Stmt> print = s => {
                if (verbose) {
                    Console.WriteLine(" --- AST Transform Step {0} ---", step++);
                    Console.WriteLine(ShowVisitor.V(method, s));
                    Console.WriteLine();
                }
            };
            Func<Func<Stmt, Stmt>, Stmt, Stmt> doo = (fn, s0) => {
                var s1 = fn(s0);
                if (s1 != s0) {
                    print(s1);
                }
                return s1;
            };
            print(stmt0);
            var stmt = doo(s => (Stmt)VisitorConvertCilToSsa.V(method, s), stmt0);
            // Reduce to AST with no continuations
            for (int i = 0; ; i++) {
                var stmtOrg = stmt;
                stmt = doo(s => (Stmt)VisitorSubstitute.V(s), stmt);
                stmt = doo(s => (Stmt)VisitorIfDistribution.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorDerecurse.V(s), stmt);
                stmt = doo(s => (Stmt)VisitorBooleanSimplification.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorIfSimplification.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorConditionRemoval.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorEmptyBlockRemoval.V(s), stmt);
                if (stmt == stmtOrg) {
                    break;
                }
                if (i > 100) {
                    // After 100 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to reduce AST");
                }
            }
            if (VisitorFindContinuations.Any(stmt)) {
                throw new InvalidOperationException("Error: Cannot reduce IL to AST with no continuations");
            }
            // Simplify AST
            for (int i = 0; ; i++) {
                var stmtOrg = stmt;
                stmt = doo(s => (Stmt)VisitorBooleanSimplification.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorIfSimplification.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorMoveOutOfLoop.V(s), stmt);
                stmt = doo(s => (Stmt)VisitorConditionRemoval.V(method, s), stmt);
                stmt = doo(s => (Stmt)VisitorSsaSimplifier.V(s), stmt);
                stmt = doo(s => (Stmt)VisitorPhiSimplifier.V(s), stmt);
                stmt = doo(s => (Stmt)VisitorExpressionSimplifier.V(method, s), stmt);
                if (stmt == stmtOrg) {
                    break;
                }
                if (i > 100) {
                    // After 100 iterations even the most complex method should be sorted out
                    throw new InvalidOperationException("Error: Stuck in loop trying to simplify AST");
                }
            }
            return stmt;
        }

    }
}
