using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class ExprJsFunction : Expr {

        public ExprJsFunction(Ctx ctx, Stmt body)
            : base(ctx) {
            this.Body = body;
        }

        public Stmt Body { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsFunction; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
