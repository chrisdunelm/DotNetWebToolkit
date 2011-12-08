using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprCall : Expr, ICall {

        public ExprCall(MethodReference calling, IEnumerable<Expr> args) {
            this.Calling = calling;
            this.Args = args;
        }

        public MethodReference Calling { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.Call; }
        }

        public override TypeReference Type {
            get { return this.Calling.ReturnType; }
        }

    }
}
