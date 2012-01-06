using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class ExprJsEmptyFunction : Expr {

        public ExprJsEmptyFunction(Ctx ctx) : base(ctx) { }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsEmptyFunction; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
