using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprNewObj : Expr, ICall {

        public ExprNewObj(Ctx ctx, MethodReference ctor, IEnumerable<Expr> args)
            : base(ctx) {
            this.CallMethod = ctor;
            this.Args = args;
        }

        public MethodReference CallMethod { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.NewObj; }
        }

        public override TypeReference Type {
            get { return this.CallMethod.DeclaringType; }
        }

        Expr ICall.Obj { get { return null; } }

    }
}
