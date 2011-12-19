using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorIfDistribution : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorIfDistribution(ast.Ctx);
            return v.Visit(ast);
        }

        class IfInfo {
            public Stack<Expr> Conditions = new Stack<Expr>();
            public List<Tuple<StmtContinuation, Expr>> AddToIf = new List<Tuple<StmtContinuation, Expr>>();
        }

        private VisitorIfDistribution(Ctx ctx) {
            this.ctx = ctx;
        }

        private Ctx ctx;
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
            this.ifInfo.Conditions.Push(this.ctx.ExprGen.NotAutoSimplify(s.Condition));
            var @else = this.Visit(s.Else);
            this.ifInfo.Conditions.Pop();
            if (then != s.Then || @else != s.Else) {
                var @if = new StmtIf(s.Ctx, s.Condition, (Stmt)then, (Stmt)@else);
                if (finalise && this.ifInfo.AddToIf.Any()) {
                    var ifStmts = this.ifInfo.AddToIf.GroupBy(x => x.Item1.To, x => x.Item2).Select(x =>
                        new StmtIf(s.Ctx, x.Aggregate((a, b) => this.ctx.ExprGen.Or(a, b)),
                            this.ifInfo.AddToIf.First(y => y.Item1.To == x.Key).Item1, null)
                        );
                    var stmts = new Stmt[] { @if }.Concat(ifStmts).ToArray();
                    this.ifInfo = null;
                    return new StmtBlock(s.Ctx, stmts);
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
            var combinedCondition = this.ifInfo.Conditions.Aggregate((a, b) => this.ctx.ExprGen.And(a, b));
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
}
