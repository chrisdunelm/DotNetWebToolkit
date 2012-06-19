using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprNewArray : Expr {

        public ExprNewArray(Ctx ctx, TypeReference elementType, Expr exprNumElements)
            : base(ctx) {
            this.ExprNumElements = exprNumElements;
            this.ElementType = elementType;
            this.type = elementType.MakeArray();
        }

        public Expr ExprNumElements { get; private set; }
        public TypeReference ElementType { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.NewArray; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override Special Specials {
            get { return Special.PossibleSideEffects; }
        }

    }
}
