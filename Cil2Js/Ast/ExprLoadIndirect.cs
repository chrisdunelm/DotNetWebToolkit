using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprLoadIndirect : Expr {

        public ExprLoadIndirect(Ctx ctx, Expr expr, TypeReference loadType)
            : base(ctx) {
            this.Expr = expr;
            this.loadType = loadType;
        }

        public Expr Expr { get; private set; }
        private TypeReference loadType;

        public override Expr.NodeType ExprType {
            get { return NodeType.LoadIndirect; }
        }

        public override TypeReference Type {
            get { return this.loadType; }
        }
    }
}
