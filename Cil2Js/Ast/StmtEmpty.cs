﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class StmtEmpty : Stmt {

        public StmtEmpty(Ctx ctx)
            : base(ctx) {
        }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Empty; }
        }

        public override string ToString() {
            return "<Empty>";
        }

    }
}
