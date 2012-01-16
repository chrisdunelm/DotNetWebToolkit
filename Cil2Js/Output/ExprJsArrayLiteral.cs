using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Output {
    public class ExprJsArrayLiteral : Expr {

        public ExprJsArrayLiteral(Ctx ctx, TypeReference elType, params Expr[] elements)
            : this(ctx, elType, (IEnumerable<Expr>)elements) {
        }

        public ExprJsArrayLiteral(Ctx ctx, TypeReference elType, IEnumerable<Expr> elements)
            : base(ctx) {
            this.Elements = elements;
            this.ElementType = elType;
            this.type = elType.MakeArray();
        }

        public IEnumerable<Expr> Elements { get; private set; }
        public TypeReference ElementType { get; private set; }
        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return (Expr.NodeType)JsExprType.JsArrayLiteral; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }
    }
}
