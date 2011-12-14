using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprArrayLength : Expr {

        public ExprArrayLength(TypeSystem typeSystem, Expr array) {
            this.TypeSystem = typeSystem;
            this.Array = array;
        }

        public TypeSystem TypeSystem { get; private set; }
        public Expr Array { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.ArrayLength; }
        }

        public override TypeReference Type {
            get { return this.TypeSystem.Int32; }
        }

    }
}
