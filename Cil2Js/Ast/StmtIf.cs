using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Cil2Js.Ast {

    [DebuggerTypeProxy(typeof(DebugView))]
    public class StmtIf : Stmt {

        class DebugView {

            public DebugView(StmtIf s) {
                this.Condition = s.Condition;
                this.Then = s.Then;
                this.Else = s.Else;
            }

            public Expr Condition { get; private set; }
            public Stmt Then { get; private set; }
            public Stmt Else { get; private set; }
        }

        public StmtIf(Ctx ctx, Expr condition, Stmt then, Stmt @else)
            : base(ctx) {
            this.Condition = condition;
            this.Then = then;
            this.Else = @else;
        }

        public Expr Condition { get; private set; }
        public Stmt Then { get; private set; }
        public Stmt Else { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.If; }
        }

    }
}
