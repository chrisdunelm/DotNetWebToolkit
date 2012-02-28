using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
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
                if (s.To.StmtType == Stmt.NodeType.Continuation && !((StmtContinuation)s.To).LeaveProtectedRegion) {
                    var newCont = new StmtContinuation(s.Ctx, ((StmtContinuation)s.To).To, false);
                    return this.Visit(newCont);
                }
                var count = VisitorCounter.GetCount(s.To, this.root);
                if (count == 1) {
                    return this.Visit(s.To);
                }
            }
            return base.VisitContinuation(s);
        }

    }
}
