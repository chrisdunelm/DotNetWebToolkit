using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprUnbox : Expr {

        public ExprUnbox(Ctx ctx, Expr expr)
            : base(ctx) {
            this.Expr = expr;
        }

        public Expr Expr { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.Unbox; }
        }

        public override TypeReference Type {
            get { return this.Expr.Type; }
        }

    }
}
