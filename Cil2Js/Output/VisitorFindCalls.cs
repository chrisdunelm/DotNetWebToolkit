using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class VisitorFindCalls : JsAstVisitor {

        public static IEnumerable<ICall> V(ICode ast) {
            var v = new VisitorFindCalls();
            v.Visit(ast);
            return v.calls;
        }

        private List<ICall> calls = new List<ICall>();

        protected override ICode VisitCall(ExprCall e) {
            this.calls.Add(e);
            return base.VisitCall(e);
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            this.calls.Add(e);
            return base.VisitNewObj(e);
        }

        protected override ICode VisitJsVirtualCall(ExprJsVirtualCall e) {
            this.calls.Add(e);
            return base.VisitJsVirtualCall(e);
        }

    }
}
