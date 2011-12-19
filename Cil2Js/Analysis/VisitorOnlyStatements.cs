using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorOnlyStatements : AstVisitor {

        public static bool Only(ICode ast, params Stmt.NodeType[] stmtTypes) {
            var v = new VisitorOnlyStatements(stmtTypes);
            v.Visit(ast);
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
}
