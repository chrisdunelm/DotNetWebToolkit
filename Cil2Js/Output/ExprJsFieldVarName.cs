using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsFieldVarName : Expr {

        public ExprJsFieldVarName(Ctx ctx, FieldReference field)
            : base(ctx) {
            this.FieldRef = field;
        }

        public FieldReference FieldRef { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsFieldVarName; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
