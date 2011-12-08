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

        public static ICode V(MethodDefinition method, ICode c) {
            var v = new VisitorConditionRemoval(method);
            return v.Visit(c);
        }

        private VisitorConditionRemoval(MethodDefinition method) {
            this.method = method;
            this.typeSystem = method.Module.TypeSystem;
            this.exprGen = Expr.ExprGen(this.typeSystem);
            this.known = new Stack<List<Tuple<Expr, Expr>>>();
            this.known.Push(new List<Tuple<Expr, Expr>>());
        }

        private MethodDefinition method;
        private TypeSystem typeSystem;
        private Expr.Gen exprGen;

        private Stack<List<Tuple<Expr, Expr>>> known;

        private Expr Bool(bool b) {
            return new ExprLiteral(b, this.typeSystem.Boolean);
        }

        private void AddKnown(Expr e, bool value) {
            var l = this.known.Peek();
            l.Add(Tuple.Create(e, this.Bool(value)));
            l.Add(Tuple.Create(this.exprGen.NotAutoSimplify(e), this.Bool(!value)));
        }

        protected override ICode VisitIf(StmtIf s) {
            var condition = (Expr)this.Visit(s.Condition);
            this.known.Push(new List<Tuple<Expr, Expr>>());
            this.AddKnown(condition, true);
            var then = (Stmt)this.Visit(s.Then);
            this.known.Pop();
            this.known.Push(new List<Tuple<Expr, Expr>>());
            this.AddKnown(condition, false);
            var @else = (Stmt)this.Visit(s.Else);
            this.known.Pop();
            if (condition != s.Condition || then != s.Then || @else != s.Else) {
                return new StmtIf(condition, then, @else);
            } else {
                return s;
            }
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var body = (Stmt)this.Visit(s.Body);
            var @while = (Expr)this.Visit(s.While); // This order matters - body must be visited before while
            this.AddKnown(@while, false);
            if (@while != s.While || body != s.Body) {
                return new StmtDoLoop(body, @while);
            } else {
                return s;
            }
        }

        protected override ICode VisitExpr(Expr e) {
            var knowns = this.known.SelectMany(x => x).ToArray();
            foreach (var known in knowns) {
                if (e.DoesEqual(known.Item1)) {
                    return known.Item2;
                }
            }
            return base.VisitExpr(e);
        }

    }
}
