using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorFindCalls:AstVisitor {

        public static IEnumerable<MethodReference> V(ICode ast) {
            var v = new VisitorFindCalls();
            v.Visit(ast);
            return v.calls;
        }

        private VisitorFindCalls() { }

        private List<MethodReference> calls = new List<MethodReference>();

        protected override ICode VisitCall(ExprCall e) {
            this.calls.Add(e.Calling);
            return base.VisitCall(e);
        }

        protected override ICode VisitCall(StmtCall s) {
            this.calls.Add(s.Calling);
            return base.VisitCall(s);
        }

    }
}
