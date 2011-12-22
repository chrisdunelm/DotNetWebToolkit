using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class ExprJsInvoke : Expr {

        public ExprJsInvoke(Ctx ctx, Expr methodToInvoke, IEnumerable<Expr> args, TypeReference returnType)
            : base(ctx) {
            this.MethodToInvoke = methodToInvoke;
            this.Args = args;
            this.returnType = returnType;
        }

        public Expr MethodToInvoke { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        private TypeReference returnType;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsInvoke; }
        }

        public override TypeReference Type {
            get { return this.returnType; }
        }

    }
}
