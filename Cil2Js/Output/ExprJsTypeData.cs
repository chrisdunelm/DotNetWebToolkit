using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsTypeData : Expr {

        public ExprJsTypeData(Ctx ctx, TypeData typeData)
            : base(ctx) {
            this.TypeData = typeData;
        }

        public TypeData TypeData { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsTypeData; }
        }

        public override TypeReference Type {
            get { throw new NotImplementedException(); }
        }

    }
}
