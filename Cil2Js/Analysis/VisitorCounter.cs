using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorCounter : AstRecursiveVisitor {

        public static int GetCount(ICode countReferences, ICode root) {
            var v = new VisitorCounter(countReferences);
            v.Visit(root);
            return v.Count;
        }

        public VisitorCounter(ICode countReferences) {
            this.countReferences = countReferences;
            this.Count = 0;
        }

        private ICode countReferences;

        public int Count { get; private set; }

        protected override ICode VisitContinuation(StmtContinuation s) {
            if (s.To == this.countReferences) {
                this.Count++;
            }
            return base.VisitContinuation(s);
        }

    }
}
