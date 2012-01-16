using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DotNetWebToolkit.Cil2Js.Ast {

    [DebuggerTypeProxy(typeof(DebugView))]
    public class StmtThrow : Stmt {

        class DebugView {

            public DebugView(StmtThrow s) {
                this.Expr = s.Expr;
            }

            public Expr Expr { get; private set; }

        }

        public StmtThrow(Ctx ctx, Expr expr)
            : base(ctx) {
            this.Expr = expr;
        }

        public Expr Expr { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Throw; }
        }

    }
}
