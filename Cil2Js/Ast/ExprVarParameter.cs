using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprVarParameter : ExprVar {

        public ExprVarParameter(Ctx ctx, ParameterDefinition parameter, TypeReference parameterType)
            : base(ctx) {
            this.Parameter = parameter;
            this.type = parameterType;
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
                return this.Parameter.ToString();
            } else {
                return "Param?";
            }
        }

    }
}
