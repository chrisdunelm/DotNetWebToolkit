using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class StmtDoLoop : Stmt {

        public StmtDoLoop(Ctx ctx, Stmt body, Expr @while)
            : base(ctx) {
            this.Body = body;
            this.While = @while;
        }

        public Stmt Body { get; private set; }
        public Expr While { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.DoLoop; }
        }

    }
}
