using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    class VisitorDefiniteAssignment : AstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorDefiniteAssignment();
            v.phiClusters = VisitorPhiClusters.V(ast);
            v.phiComparer = new VisitorPhiClusters.EqualityComparer(v.phiClusters);
            v.stack.Push(new List<Expr>());
            return v.Visit(ast);
        }

        private IEnumerable<IEnumerable<Expr>> phiClusters;
        private IEqualityComparer<Expr> phiComparer;
        private Stack<List<Expr>> stack = new Stack<List<Expr>>();

        protected override ICode VisitAssignment(ExprAssignment e) {
            this.stack.Peek().Add(e.Target);
            this.Visit(e.Expr);
            return e;
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            this.stack.Peek().Add(s.Target);
            this.Visit(s.Expr);
            return s;
        }

        protected override ICode VisitIf(StmtIf s) {
            var condition = (Expr)this.Visit(s.Condition);

            this.stack.Push(new List<Expr>());
            var then = (Stmt)this.Visit(s.Then);
            var thenDA = this.stack.Pop();

            this.stack.Push(new List<Expr>());
            var @else = (Stmt)this.Visit(s.Else);
            var elseDA = this.stack.Pop();

            var intersection = thenDA.Intersect(elseDA, this.phiComparer).ToArray();
            this.stack.Peek().AddRange(intersection);

            if (condition != s.Condition || then != s.Then || @else != s.Else) {
                return new StmtIf(s.Ctx, condition, then, @else);
            } else {
                return s;
            }
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var @while = (Expr)this.Visit(s.While);
            var body = (Stmt)this.Visit(s.Body);

            // Make sure that all variables used in the 'while' condition are assigned within the loop.
            // If not, then at the beginning of the loop assign them their default values
            var whileVars = VisitorBooleanSimplification.GetVarsVisitor.GetAll(@while);
            var allDefinitelyAssigned = this.stack.SelectMany(x => x).ToArray();
            var notDefinitelyAssigned = whileVars
                .Where(x => x.IsVar() && !allDefinitelyAssigned.Contains(x, this.phiComparer)).ToArray();

            if (notDefinitelyAssigned.Any()) {
                var ctx = s.Ctx;
                var defaultStmts = notDefinitelyAssigned
                    .Select(x => new {
                        stmt = new StmtAssignment(ctx, ctx.Local(x.Type), new ExprDefaultValue(ctx, x.Type)),
                        var = x
                    })
                    .ToArray();
                body = new StmtBlock(ctx, defaultStmts.Select(x => (Stmt)x.stmt).Concat(body));
                foreach (var defaultStmt in defaultStmts) {
                    var phi = new ExprVarPhi(ctx) {
                        Exprs = new[] { defaultStmt.var, defaultStmt.stmt.Target }
                    };
                    @while = (Expr)VisitorReplace.V(@while, defaultStmt.var, phi);
                }
            }

            if (@while != s.While || body != s.Body) {
                return new StmtDoLoop(s.Ctx, body, @while);
            } else {
                return s;
            }
        }

    }
}
