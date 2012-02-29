using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorContToCounter : AstRecursiveVisitor {

        public static int GetCount(ICode countReferences, ICode root) {
            var v = new VisitorContToCounter(countReferences);
            v.Visit(root);
            return v.Count;
        }

        public VisitorContToCounter(ICode countReferences) {
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
