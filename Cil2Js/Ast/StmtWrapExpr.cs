using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class StmtWrapExpr : Stmt {

        public StmtWrapExpr(Ctx ctx, Expr expr)
            : base(ctx) {
            this.Expr = expr;
        }

        public Expr Expr { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.WrapExpr; }
        }

    }
}
