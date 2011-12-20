using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class VisitorFindCalls : JsAstVisitor {

        public static IEnumerable<Tuple<ICall, bool>> V(ICode ast) {
            var v = new VisitorFindCalls();
            v.Visit(ast);
            return v.calls;
        }

        private List<Tuple<ICall, bool>> calls = new List<Tuple<ICall, bool>>();

        protected override ICode VisitCall(ExprCall e) {
            this.calls.Add(Tuple.Create((ICall)e, e.IsVirtual));
            return base.VisitCall(e);
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            this.calls.Add(Tuple.Create((ICall)e, false));
            return base.VisitNewObj(e);
        }

    }
}
