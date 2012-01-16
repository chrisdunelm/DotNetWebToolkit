using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsVarMethodReference : ExprVar {

        public ExprJsVarMethodReference(Ctx ctx, MethodReference mRef)
            : base(ctx) {
            this.MRef = mRef;
        }

        public MethodReference MRef { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsVarMethodReference; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
