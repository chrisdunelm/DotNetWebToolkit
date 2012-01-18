using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class ExprJsExplicit : Expr {

        public ExprJsExplicit(Ctx ctx, string js, TypeReference type, params Expr[] exprs)
            : this(ctx, js, type, (IEnumerable<Expr>)exprs) {
        }

        public ExprJsExplicit(Ctx ctx, string js, TypeReference type, IEnumerable<Expr> exprs)
            : base(ctx) {
            this.JavaScript = js;
            this.type = type;
            this.Exprs = exprs;
        }

        public string JavaScript { get; private set; }
        public IEnumerable<Expr> Exprs { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsExplicit; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }
    }
}
