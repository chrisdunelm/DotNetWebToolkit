using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprFieldAccess : ExprVar {

        public ExprFieldAccess(Expr obj, FieldDefinition field) {
            this.Obj = obj;
            this.Field = field;
        }

        public Expr Obj { get; private set; }
        public FieldDefinition Field { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.LoadField; }
        }

        public override TypeReference Type {
            get { return this.Field.FieldType; }
        }

        public override string ToString() {
            return string.Format("{0}.{1}", this.Obj, this.Field.Name);
        }

    }
}
