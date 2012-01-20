using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprElementAddress : Expr {

        public ExprElementAddress(Ctx ctx, Expr array, Expr index, TypeReference elementType)
            : base(ctx) {
            this.Array = array;
            this.Index = index;
            this.type = elementType.MakePointer();
        }

        public Expr Array { get; private set; }
        public Expr Index { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.ElementAddress; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }
}
