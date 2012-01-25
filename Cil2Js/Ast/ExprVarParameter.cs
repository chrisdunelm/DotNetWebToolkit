using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprVarParameter : ExprVar {

        public ExprVarParameter(Ctx ctx, ParameterDefinition parameter)
            : base(ctx) {
            this.Parameter = parameter;
            this.type = parameter.ParameterType;
        }

        public ParameterDefinition Parameter { get; private set; }
        private TypeReference type;

        public override NodeType ExprType {
            get { return NodeType.VarParameter; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            if (this.Parameter != null) {
                var s = this.Parameter.ToString();
                if (string.IsNullOrWhiteSpace(s)) {
                    s = "param_" + this.Parameter.Index;
                }
                return s;
            } else {
                return "Param?";
            }
        }

    }
}
