using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {
    public class StmtBreak : Stmt {

        public StmtBreak(Ctx ctx) : base(ctx) { }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Break; }
        }

    }
}
