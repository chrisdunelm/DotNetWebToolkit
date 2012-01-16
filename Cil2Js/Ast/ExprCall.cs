using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Ast {

    public class ExprCall : Expr, ICall {

        public ExprCall(Ctx ctx, MethodReference callMethod, Expr obj, IEnumerable<Expr> args, bool isVirtualCall)
            : base(ctx) {
            this.CallMethod = callMethod;
            this.Obj = obj;
            this.Args = args;
            this.IsVirtualCall = isVirtualCall;
            this.returnType = callMethod.ReturnType.FullResolve(callMethod);
        }

        public Expr Obj { get; private set; }
        public MethodReference CallMethod { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }
        public bool IsVirtualCall { get; private set; }

        private TypeReference returnType;

        public bool IsStatic {
            get { return this.Obj == null; }
        }

        public override Expr.NodeType ExprType {
            get { return NodeType.Call; }
        }

        public override TypeReference Type {
            get { return this.returnType; }
        }

        public override Special Specials {
            get { return Special.PossibleSideEffects; }
        }

        public override string ToString() {
            return string.Format("{0}.{1}({2})", this.IsStatic ? (object)this.CallMethod.DeclaringType : this.Obj, this.CallMethod.Name,
                string.Join(", ", this.Args));
        }

    }
}
