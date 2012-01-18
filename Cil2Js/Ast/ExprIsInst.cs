using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprIsInst : Expr {

        public ExprIsInst(Ctx ctx, Expr expr, TypeReference toType)
            : base(ctx) {
            this.Expr = expr;
            this.toType = toType;
        }

        public Expr Expr { get; private set; }
        private TypeReference toType;

        public override Expr.NodeType ExprType {
            get { return NodeType.IsInst; }
        }

        public override TypeReference Type {
            get { return this.toType; }
        }

    }
}
