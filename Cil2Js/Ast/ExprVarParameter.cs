using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprVarParameter : ExprVar {

        public ExprVarParameter(ParameterDefinition parameter) {
            this.Parameter = parameter;
        }

        public ParameterDefinition Parameter { get; private set; }

        public override NodeType ExprType {
            get { return NodeType.VarParameter; }
        }

        public override TypeReference Type {
            get { return this.Parameter.ParameterType; }
        }

        public override string ToString() {
            if (this.Parameter != null) {
                return this.Parameter.ToString();
            } else {
                return "Param?";
            }
        }

    }
}
