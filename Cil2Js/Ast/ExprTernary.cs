using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class ExprTernary : Expr {

        public ExprTernary(Ctx ctx, Expr condition, Expr ifTrue, Expr ifFalse)
            : base(ctx) {
            this.Condition = condition;
            this.IfTrue = ifTrue;
            this.IfFalse = ifFalse;
            this.type = TypeCombiner.Combine(ctx, ifTrue, ifFalse);
        }

        public Expr Condition { get; private set; }
        public Expr IfTrue { get; private set; }
        public Expr IfFalse { get; private set; }

        private TypeReference type;

        public override Expr.NodeType ExprType {
            get { return NodeType.Ternary; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

    }
}
