using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsDelegateCtor : Expr {

        public ExprJsDelegateCtor(Ctx ctx, TypeReference delegateType, Expr obj, MethodReference method)
            : base(ctx) {
            this.Obj = obj;
            this.Method = method;
            this.delegateType = delegateType;
        }

        public Expr Obj { get; private set; }
        public MethodReference Method { get; private set; }
        private TypeReference delegateType;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsDelegateCtor; }
        }

        public override TypeReference Type {
            get { return this.delegateType; }
        }

    }
}
