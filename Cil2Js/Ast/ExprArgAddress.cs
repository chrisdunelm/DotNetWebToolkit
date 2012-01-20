using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprArgAddress : Expr {

        public ExprArgAddress(Ctx ctx, int index, TypeReference type)
            : base(ctx) {
            this.Index = index;
            this.type = type.MakePointer();
        }

        public int Index { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.ArgAddress; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }
}
