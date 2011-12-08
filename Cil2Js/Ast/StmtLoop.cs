using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {
    public class StmtDoLoop : Stmt {

        public StmtDoLoop(Stmt body, Expr @while) {
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
