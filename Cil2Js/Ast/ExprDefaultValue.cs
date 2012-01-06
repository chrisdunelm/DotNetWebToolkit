using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprDefaultValue : Expr {

        public ExprDefaultValue(Ctx ctx, TypeReference type)
            : base(ctx) {
            this.type = type;
        }

        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.DefaultValue; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return string.Format("DefaultValue({0})", this.Type);
        }

    }
}
