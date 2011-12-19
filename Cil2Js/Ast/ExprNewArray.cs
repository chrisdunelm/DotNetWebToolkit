using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprNewArray : Expr {

        public ExprNewArray(Ctx ctx, TypeReference elementType, Expr exprNumElements)
            : base(ctx) {
            this.ExprNumElements = exprNumElements;
            this.type = new ArrayType(elementType);
        }

        public Expr ExprNumElements { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.NewArray; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }
    }
}
