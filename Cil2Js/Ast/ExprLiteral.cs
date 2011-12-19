using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprLiteral : Expr {

        public ExprLiteral(Ctx ctx, object value, TypeReference type)
            : base(ctx) {
            this.Value = value;
            this.type = type;
        }

        public object Value { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.Literal; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return this.Value.ToString();
        }

    }
}
