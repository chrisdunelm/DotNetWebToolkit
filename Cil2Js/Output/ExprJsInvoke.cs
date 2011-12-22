using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class ExprJsInvoke : Expr {

        public ExprJsInvoke(Ctx ctx)
            : base(ctx) {
        }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsInvoke; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
