using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsFunction : Expr {

        public ExprJsFunction(Ctx ctx, IEnumerable<Expr> args, Stmt body)
            : base(ctx) {
            this.Args = args;
            this.Body = body;
        }

        public IEnumerable<Expr> Args { get; private set; }
        public Stmt Body { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsFunction; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
