using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprCast : Expr {

        public ExprCast(Ctx ctx, Expr expr, TypeReference castTo)
            : base(ctx) {
            this.Expr = expr;
            this.castTo = castTo;
        }

        public Expr Expr { get; private set; }
        private TypeReference castTo;

        public override Expr.NodeType ExprType {
            get { return NodeType.Cast; }
        }

        public override TypeReference Type {
            get { return this.castTo; }
        }

        public override string ToString() {
            return string.Format("({0}){1}", this.castTo, this.Expr);
        }

    }
}
