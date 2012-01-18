using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsTypeVarName : Expr {

        public ExprJsTypeVarName(Ctx ctx, TypeReference type)
            : base(ctx) {
            this.TypeRef = type;
        }

        public TypeReference TypeRef { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsTypeVarName; }
        }

        public override TypeReference Type {
            get { return this.Ctx.Module.Import(typeof(Type)); }
        }
    }
}
