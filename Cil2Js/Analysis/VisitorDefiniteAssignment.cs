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
            var phiClusters = VisitorPhiClusters.V(ast);
            var v = new VisitorDefiniteAssignment {
                phiComparer = new VisitorPhiClusters.EqualityComparer(phiClusters),
            };
            v.stack.Push(new List<ExprVar>());
            return v.Visit(ast);
        }

        private IEqualityComparer<Expr> phiComparer;
        private Stack<List<ExprVar>> stack = new Stack<List<ExprVar>>();

        private Dictionary<Stmt, IEnumerable<ExprVar>> ensureAssigned = new Dictionary<Stmt, IEnumerable<ExprVar>>();
        private Dictionary<Stmt, Stmt> stmtMap = new Dictionary<Stmt, Stmt>();

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
            var ctx = s.Ctx;
            var condition = (Expr)this.Visit(s.Condition);

            this.stack.Push(new List<ExprVar>());
            var then = (Stmt)this.Visit(s.Then);
            var thenDA = this.stack.Pop();

            this.stack.Push(new List<ExprVar>());
            var @else = (Stmt)this.Visit(s.Else);
            var elseDA = this.stack.Pop();

            var intersection = thenDA.Intersect(elseDA, (IEqualityComparer<ExprVar>)this.phiComparer).ToArray();
            this.stack.Peek().AddRange(intersection);

            var conditionVars = VisitorFindVars.V(condition);
            var needAssigning = conditionVars.Except(this.stack.SelectMany(x => x), (IEqualityComparer<ExprVar>)this.phiComparer).ToArray();
            if (needAssigning.Any()) {
                var replacements = needAssigning.Select(x => {
                    var newExpr = ctx.Local(x.Type);
                    var phi = new ExprVarPhi(ctx) { Exprs = new[] { x, newExpr } };
                    return new { orgExpr = x, newExpr, phi };
                }).ToArray();
                foreach (var replace in replacements) {
                    this.stack.Peek().Add(replace.newExpr);
                    this.stack.Peek().Add(replace.phi);
                    condition = (Expr)VisitorReplace.V(condition, replace.orgExpr, replace.phi);
                }
                this.ensureAssigned.Add(s, replacements.Select(x => x.newExpr).ToArray());
            }

            if (condition != s.Condition || then != s.Then || @else != s.Else) {
                var newS = new StmtIf(ctx, condition, then, @else);
                this.stmtMap.Add(newS,s);
                return newS;
            } else {
                return s;
            }
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var ctx = s.Ctx;
            var body = (Stmt)this.Visit(s.Body);
            var @while = (Expr)this.Visit(s.While);

            var conditionVars = VisitorFindVars.V(@while);
            var needAssigning = conditionVars.Except(this.stack.SelectMany(x => x), (IEqualityComparer<ExprVar>)this.phiComparer).ToArray();
            if (needAssigning.Any()) {
                var replacements = needAssigning.Select(x => {
                    var newExpr = ctx.Local(x.Type);
                    var phi = new ExprVarPhi(ctx) { Exprs = new[] { x, newExpr } };
                    return new { orgExpr = x, newExpr, phi };
                }).ToArray();
                foreach (var replace in replacements) {
                    this.stack.Peek().Add(replace.newExpr);
                    this.stack.Peek().Add(replace.phi);
                    @while = (Expr)VisitorReplace.V(@while, replace.orgExpr, replace.phi);
                }
                var assignmentStmts = replacements
                    .Select(x=>new StmtAssignment(ctx, x.newExpr, new ExprDefaultValue(ctx, x.newExpr.Type)))
                    .ToArray();
                body = new StmtBlock(ctx, assignmentStmts.Concat(body));
            }

            if (body != s.Body || @while != s.While) {
                return new StmtDoLoop(ctx, body, @while);
            } else {
                return s;
            }
        }

        protected override ICode VisitBlock(StmtBlock s) {
            var ctx = s.Ctx;
            var sBlock = (StmtBlock)base.VisitBlock(s);
            var assignments = sBlock.Statements
                .SelectMany(x => this.ensureAssigned.ValueOrDefault(this.stmtMap.ValueOrDefault(x, x), Enumerable.Empty<ExprVar>()))
                .ToArray();
            if (assignments.Any()) {
                var assignmentStmts = assignments
                    .Select(x => new StmtAssignment(ctx, x, new ExprDefaultValue(ctx, x.Type)))
                    .ToArray();
                return new StmtBlock(ctx, assignmentStmts.Concat(sBlock.Statements));
            } else {
                return sBlock;
            }
        }

    }
}
