using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprAssignment : Expr {

        public ExprAssignment(Ctx ctx, ExprVar target, Expr expr)
            : base(ctx) {
            this.Target = target;
            this.Expr = expr;
        }

        public ExprVar Target { get; private set; }
        public Expr Expr { get; private set; }

        public override Expr.NodeType ExprType { get { return NodeType.Assignment; } }

        public override TypeReference Type {
            get { return this.Expr.Type; }
        }

        public override string ToString() {
            return string.Format("{0} = {1}", this.Target, this.Expr);
        }

    }
}
