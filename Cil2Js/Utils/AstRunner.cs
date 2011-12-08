using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using System.Dynamic;

namespace Cil2Js.Utils {
    public class AstRunner : AstVisitor {

        public static object Run(ICode c, params object[] args) {
            var v = new AstRunner(args);
            v.Visit(c);
            return v.stack.Any() ? v.stack.Pop() : null;
        }

        private AstRunner(object[] args)
            : base(true) {
            this.args = args;
        }

        class VarRecord {
            private static long order = 0;
            public VarRecord(dynamic value, bool order0 = false) {
                this.Value = value;
                this.Order = order0 ? 0 : ++order;
            }
            public dynamic Value { get; private set; }
            public long Order { get; private set; }
            public override string ToString() {
                return string.Format("[{0}] {1}", this.Order, this.Value);
            }
        }

        private object[] args;
        private bool returnNow = false;
        private Stack<dynamic> stack = new Stack<dynamic>();
        private Dictionary<ExprVar, VarRecord> vars = new Dictionary<ExprVar, VarRecord>();

        public override ICode Visit(ICode c) {
            if (this.returnNow) {
                return c;
            }
            return base.Visit(c);
        }

        protected override ICode VisitReturn(StmtReturn s) {
            this.Visit(s.Expr);
            this.returnNow = true;
            return s;
        }

        protected override ICode VisitBinary(ExprBinary e) {
            this.Visit(e.Left);
            this.Visit(e.Right);
            var right = this.stack.Pop();
            var left = this.stack.Pop();
            dynamic r;
            switch (e.Op) {
            case BinaryOp.Add: r = left + right; break;
            case BinaryOp.Sub: r = left - right; break;
            case BinaryOp.Mul: r = left * right; break;
            case BinaryOp.Div: r = left / right; break;
            case BinaryOp.LessThan: r = left < right; break;
            case BinaryOp.GreaterThan: r = left > right; break;
            default: throw new NotImplementedException("Cannot handle: " + e.Op);
            }
            this.stack.Push(r);
            return e;
        }

        protected override ICode VisitBlock(StmtBlock s) {
            foreach (var stmt in s.Statements) {
                this.Visit(stmt);
            }
            return s;
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            this.Visit(s.Expr);
            this.vars[s.Target] = new VarRecord(this.stack.Pop());
            return s;
        }

        protected override ICode VisitLiteral(ExprLiteral e) {
            this.stack.Push(e.Value);
            return e;
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            dynamic condition;
            do {
                this.Visit(s.Body);
                this.Visit(s.While);
                condition = this.stack.Pop();
            } while (condition);
            return s;
        }

        protected override ICode VisitVarPhi(ExprVarPhi e) {
            var value = e.Exprs
                .Select(x => {
                    if (x.ExprType == Expr.NodeType.VarParameter) {
                        var p = (ExprVarParameter)x;
                        return new VarRecord(this.args[p.Parameter.Index], true);
                    } else {
                        return this.vars.ValueOrDefault((ExprVar)x);
                    }
                })
                .Where(x => x != null)
                .OrderByDescending(x => x.Order)
                .First()
                .Value;
            this.stack.Push(value);
            return e;
        }

        protected override ICode VisitVarParameter(ExprVarParameter e) {
            var idx = e.Parameter.Index;
            this.stack.Push(this.args[idx]);
            return e;
        }

        protected override ICode VisitVarLocal(ExprVarLocal e) {
            this.stack.Push(this.vars[e].Value);
            return e;
        }

        protected override ICode VisitIf(StmtIf s) {
            this.Visit(s.Condition);
            var condition = this.stack.Pop();
            if (condition) {
                this.Visit(s.Then);
            } else {
                this.Visit(s.Else);
            }
            return s;
        }

    }
}
