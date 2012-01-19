using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprFieldAddress : Expr {

        public ExprFieldAddress(Ctx ctx, Expr obj, FieldReference field)
            : base(ctx) {
            this.Obj = obj;
            this.Field = field;
            this.type = field.FieldType.FullResolve(field);
        }

        private TypeReference type;
        public Expr Obj { get; private set; }
        public FieldReference Field { get; private set; }

        public bool IsStatic {
            get { return this.Obj == null; }
        }

        public override Expr.NodeType ExprType {
            get { return NodeType.FieldAddress; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }
    }
}
