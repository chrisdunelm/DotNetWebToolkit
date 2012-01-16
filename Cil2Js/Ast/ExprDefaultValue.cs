using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprDefaultValue : Expr {

        public ExprDefaultValue(Ctx ctx, TypeReference type)
            : base(ctx) {
            //if (type.IsGenericParameter) {
            //    throw new ArgumentException("Cannot be generic parameter");
            //}
            this.type = type.FullResolve(ctx);
        }

        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.DefaultValue; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return string.Format("DefaultValue({0})", this.Type);
        }

    }
}
