using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Analysis;

namespace Cil2Js.Ast {
    public class ExprVarPhi : ExprVar {

        public ExprVarPhi(Ctx ctx)
            : base(ctx) {
        }

        public override Expr.NodeType ExprType {
            get { return NodeType.VarPhi; }
        }

        public override TypeReference Type {
            get {
                if (!this.Exprs.Any()) {
                    return this.Ctx.Object.Resolve();
                }
                var t = this.Exprs
                    .Select(x => x.Type)
                    .Aggregate((a, b) => TypeCombiner.Combine(this.Ctx, a, b));
                return t;
            }
        }

        public IEnumerable<Expr> Exprs { get; set; }

        public override string ToString() {
            //return string.Format("phi({0})", string.Join(", ", this.Exprs.Select(x => x.ToString())));
            return string.Format("phi<{1}>(...[{0}])", this.Exprs.Count(), this.GetHashCode());
        }

    }
}
