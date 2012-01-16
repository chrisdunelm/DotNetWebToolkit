using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class StmtSwitch : Stmt {

        public class Case {
            public Case(int value, Stmt stmt) {
                this.Value = value;
                this.Stmt = stmt;
            }
            public int Value { get; private set; }
            public Stmt Stmt { get; private set; }
        }

        public StmtSwitch(Ctx ctx, Expr expr, IEnumerable<Case> cases, Stmt @default)
            : base(ctx) {
            this.Expr = expr;
            this.Cases = cases;
            this.Default = @default;
        }

        public Expr Expr { get; private set; }
        public IEnumerable<Case> Cases { get; private set; }
        public Stmt Default { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Switch; }
        }

    }
}
