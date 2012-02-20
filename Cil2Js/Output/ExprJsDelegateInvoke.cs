using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsDelegateInvoke : Expr {

        public ExprJsDelegateInvoke(Ctx ctx, Expr methodToInvoke, IEnumerable<Expr> args)
            : base(ctx) {
            this.MethodToInvoke = methodToInvoke;
            this.Args = args;
        }

        public Expr MethodToInvoke { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsDelegateInvoke; }
        }

        public override TypeReference Type {
            get { return this.MethodToInvoke.Type; }
        }

    }
}
