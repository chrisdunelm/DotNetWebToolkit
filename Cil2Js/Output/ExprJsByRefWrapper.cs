using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsByRefWrapper : Expr {

        public ExprJsByRefWrapper(Ctx ctx, Expr expr, ExprVarLocal resultTemp, IEnumerable<Tuple<Expr, Expr>> byRefs)
            : base(ctx) {
            this.Expr = expr;
            this.ResultTemp = resultTemp;
            this.ByRefs = byRefs;
        }

        public Expr Expr { get; private set; }
        public ExprVarLocal ResultTemp { get; private set; }
        public IEnumerable<Tuple<Expr, Expr>> ByRefs { get; private set; }

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsByRefWrapper; }
        }

        public override TypeReference Type {
            get { return this.Expr.Type; }
        }

    }
}
