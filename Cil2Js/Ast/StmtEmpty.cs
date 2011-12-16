using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {
    public class StmtEmpty :Stmt {

        public override Stmt.NodeType StmtType {
            get { return NodeType.Empty; }
        }

        public override string ToString() {
            return "<Empty>";
        }

    }
}
