using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprVariableAddress : Expr {

        public ExprVariableAddress(Ctx ctx, ExprVar variable)
            : base(ctx) {
            this.Variable = variable;
            this.type = ctx.Module.Import(typeof(IntPtr));
        }

        public ExprVar Variable { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.VariableAddress; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }
}
