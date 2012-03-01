using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorFindContinuationsRecursive : AstRecursiveVisitor {

        public static bool Any(ICode ast) {
            var v = new VisitorFindContinuationsRecursive();
            v.Visit(ast);
            return v.Continuations.Any();
        }

        public static IEnumerable<StmtContinuation> Get(ICode ast) {
            var v = new VisitorFindContinuationsRecursive();
            v.Visit(ast);
            return v.Continuations;
        }

        private List<StmtContinuation> continuations = new List<StmtContinuation>();
        public IEnumerable<StmtContinuation> Continuations { get { return this.continuations; } }

        protected override ICode VisitContinuation(StmtContinuation s) {
            this.continuations.Add(s);
            return base.VisitContinuation(s);
        }

    }
}
