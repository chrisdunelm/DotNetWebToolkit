using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprUnbox : Expr {

        public ExprUnbox(Ctx ctx, Expr expr, TypeReference type)
            : base(ctx) {
            this.Expr = expr;
            this.type = type;
        }

        public Expr Expr { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.Unbox; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }
}
