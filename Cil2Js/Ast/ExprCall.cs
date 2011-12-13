using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {

    public class ExprCall : Expr, ICall {

        public ExprCall(MethodDefinition callMethod, Expr obj, IEnumerable<Expr> args, bool isVirtual) {
            this.CallMethod = callMethod;
            this.Obj = obj;
            this.Args = args;
            this.IsVirtual = isVirtual;
        }

        public Expr Obj { get; private set; }
        public MethodDefinition CallMethod { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        public bool IsVirtual { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.Call; }
        }

        public override TypeReference Type {
            get { return this.CallMethod.ReturnType; }
        }

        public override Special Specials {
            get { return Special.PossibleSideEffects; }
        }

    }
}
