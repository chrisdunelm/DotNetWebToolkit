using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {
    public class StmtAssignment : Stmt {

        public StmtAssignment(Ctx ctx, ExprVar target, Expr expr)
            : base(ctx) {
            this.Target = target;
            this.Expr = expr;
        }

        public ExprVar Target { get; private set; }
        public Expr Expr { get; private set; }

        public override Stmt.NodeType StmtType { get { return NodeType.Assignment; } }

        public override string ToString() {
            return string.Format("{0} = {1}", this.Target, this.Expr);
        }

    }
}
