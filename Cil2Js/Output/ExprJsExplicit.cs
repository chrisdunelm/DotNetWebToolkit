using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsExplicit : Expr {

        public ExprJsExplicit(Ctx ctx, string js, TypeReference type, params NamedExpr[] namedExprs)
            : this(ctx, js, type, (IEnumerable<NamedExpr>)namedExprs) {
        }

        public ExprJsExplicit(Ctx ctx, string js, TypeReference type, IEnumerable<NamedExpr> namedExprs)
            : base(ctx) {
            this.JavaScript = js;
            this.type = type;
            this.NamedExprs = namedExprs;
        }

        public string JavaScript { get; private set; }
        public IEnumerable<NamedExpr> NamedExprs { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsExplicit; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }
    }
}
