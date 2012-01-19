using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprConv : Expr {

        public ExprConv(Ctx ctx, Expr expr, TypeReference convTo)
            : base(ctx) {
            this.Expr = expr;
            this.convTo = convTo;
        }

        public Expr Expr { get; private set; }
        private TypeReference convTo;

        public override Expr.NodeType ExprType {
            get { return NodeType.Conv; }
        }

        public override TypeReference Type {
            get { return this.convTo; }
        }

    }
}
