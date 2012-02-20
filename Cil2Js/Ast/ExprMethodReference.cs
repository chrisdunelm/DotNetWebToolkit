using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class ExprMethodReference : Expr, ICall {

        public ExprMethodReference(Ctx ctx, MethodReference method)
            : base(ctx) {
            this.Method = method;
        }

        public MethodReference Method { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.MethodReference; }
        }

        public override TypeReference Type {
            get { return this.Ctx.IntPtr; }
        }

        MethodReference ICall.CallMethod {
            get { return this.Method; }
        }

        bool ICall.IsVirtualCall {
            get { return false; }
        }

        Expr ICall.Obj {
            get { return null; }
        }

        IEnumerable<Expr> ICall.Args {
            get { return Enumerable.Empty<Expr>(); }
        }

    }
}
