using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprVarLocal : ExprVar {

        public ExprVarLocal(VariableDefinition var) {
            this.Var = var;
        }

        public ExprVarLocal(TypeReference type, string name = null) {
            this.Var = new VariableDefinition(name, type);
        }

        public ExprVarLocal() {
            this.Var = null;
        }

        public VariableDefinition Var { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.VarLocal; }
        }

        public override TypeReference Type {
            get { return this.Var.VariableType; }
        }

        public override string ToString() {
            if (this.Var != null && !string.IsNullOrWhiteSpace(this.Var.Name)) {
                return this.Var.ToString();
            } else {
                return string.Format("Var_{0:x8}:{1}", this.GetHashCode(), this.Type.Name);
            }
        }

    }
}
