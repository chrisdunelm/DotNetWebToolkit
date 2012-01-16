using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprBox : Expr {

        public ExprBox(Ctx ctx, Expr expr)
            : base(ctx) {
            this.Expr = expr;
        }

        public Expr Expr { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.Box; }
        }

        public override TypeReference Type {
            get { return this.Expr.Type; }
        }

    }
}
