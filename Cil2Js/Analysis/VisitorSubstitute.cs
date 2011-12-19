using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorSubstitute : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorSubstitute(ast);
            return v.Visit(ast);
        }

        public VisitorSubstitute(ICode root) {
            this.root = root;
        }

        private ICode root;

        protected override ICode VisitContinuation(StmtContinuation s) {
            if (!s.LeaveProtectedRegion) {
                // Must never substitute when leaving protected region.
                // This would change which statements were inside the try/catch/finally region
                var count = VisitCounter.GetCount(s.To, this.root);
                if (count == 1) {
                    return this.Visit(s.To);
                }
            }
            return base.VisitContinuation(s);
        }

    }
}
