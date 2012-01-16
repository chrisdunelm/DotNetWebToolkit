using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprVarThis : ExprVar {

        public ExprVarThis(Ctx ctx, TypeReference type)
            : base(ctx) {
            this.type = type;
        }

        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.VarThis; }
        }

        public override Mono.Cecil.TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return "this";
        }

    }
}
