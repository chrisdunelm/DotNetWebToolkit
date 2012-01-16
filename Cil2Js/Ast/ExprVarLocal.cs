using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Ast {
    public class ExprVarLocal : ExprVar {

        public ExprVarLocal(Ctx ctx, TypeReference type, string name = null)
            : base(ctx) {
            this.type = type.NullThru(x => x.FullResolve(ctx));
            this.Name = name;
        }

        private TypeReference type;

        public string Name { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.VarLocal; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return string.Format("Var_{0:x8}:{1}{2}", this.GetHashCode(), this.Type.Name,
                this.Name == null ? "" : " \"" + this.Name + "\"");
        }

    }
}
