using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Analysis {
    public class VisitorConditionRemoval : AstRecursiveVisitor {
        // Removes conditions that are known to be true,
        // because previous 'if' or 'do' statements have already checked them

        public static ICode V(ICode ast) {
            var v = new VisitorConditionRemoval(ast.Ctx);
            return v.Visit(ast);
        }

        private VisitorConditionRemoval(Ctx ctx) {
            this.ctx = ctx;
            this.knownTrue = new Stack<List<Expr>>();
            this.knownTrue.Push(new List<Expr>());
        }

        private Ctx ctx;

        private Stack<List<Expr>> knownTrue;

        //private Expr Bool(bool b) {
        //    return new ExprLiteral(this.ctx, b, this.ctx.Boolean);
        //}

        private void AddKnownTrue(Expr e) {
            var l = this.knownTrue.Peek();
            l.Add(e);
            // TODO: This following code causes a bug in test TestLoops::Test4ForBreakAndContinue() - why?
            //if (e.ExprType == Expr.NodeType.Binary) {
            //    var eBin = (ExprBinary)e;
            //    if (eBin.Op == BinaryOp.Or) {
            //        this.AddKnownTrue(eBin.Left);
            //        this.AddKnownTrue(eBin.Right);
            //    }
            //}
        }

        private bool HasBlockExit(Stmt s) {
            if (s == null) {
                return false;
            }
            if (s.StmtType == Stmt.NodeType.Throw || s.StmtType == Stmt.NodeType.Return || s.StmtType == Stmt.NodeType.Continuation) {
                return true;
            }
            if (s.StmtType == Stmt.NodeType.Block) {
                return ((StmtBlock)s).Statements.Any(x => x.StmtType == Stmt.NodeType.Throw || x.StmtType == Stmt.NodeType.Return || x.StmtType == Stmt.NodeType.Continuation);
            }
            return false;
        }

        protected override ICode VisitIf(StmtIf s) {
            var condition = (Expr)this.Visit(s.Condition);
            this.knownTrue.Push(new List<Expr>());
            this.AddKnownTrue(condition);
            var then = (Stmt)this.Visit(s.Then);
            var thenTrue = this.knownTrue.Pop();
            this.knownTrue.Push(new List<Expr>());
            this.AddKnownTrue(s.Ctx.ExprGen.Not(condition));
            var @else = (Stmt)this.Visit(s.Else);
            var elseTrue = this.knownTrue.Pop();
            if (this.HasBlockExit(s.Then)) {
                //this.knownTrue.Peek().AddRange(elseTrue);
                this.AddKnownTrue(s.Ctx.ExprGen.Not(condition));
                //this.AddKnownTrue(elseTrue);
            }
            if (this.HasBlockExit(s.Else)) {
                //this.knownTrue.Peek().AddRange(thenTrue);
                //this.AddKnownTrue(condition);
                this.AddKnownTrue(condition);
            }
            if (condition != s.Condition || then != s.Then || @else != s.Else) {
                return new StmtIf(this.ctx, condition, then, @else);
            } else {
                return s;
            }
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var body = (Stmt)this.Visit(s.Body);
            var @while = (Expr)this.Visit(s.While); // This order matters - body must be visited before while
            this.AddKnownTrue(s.Ctx.ExprGen.NotAutoSimplify(@while));
            if (@while != s.While || body != s.Body) {
                return new StmtDoLoop(this.ctx, body, @while);
            } else {
                return s;
            }
        }

        protected override ICode VisitExpr(Expr e) {
            //var knownTrues = this.knownTrue.SelectMany(x => x).ToArray();
            //foreach (var knownTrue in knownTrues) {
            //    if (e.DoesEqual(knownTrue)) {
            //        return new ExprLiteral(e.Ctx, true, e.Ctx.Boolean);
            //    }
            //    if (e.DoesEqualNot(knownTrue)) {
            //        return new ExprLiteral(e.Ctx, false, e.Ctx.Boolean);
            //    }
            //}
            return base.VisitExpr(e);
        }

    }
}
