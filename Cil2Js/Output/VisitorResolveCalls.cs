using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public class VisitorResolveCalls : AstVisitor {

        public VisitorResolveCalls(Func<ICall, JsResolved> fnCallResolver) {
            this.fnCallResolver = fnCallResolver;
        }

        //public IEnumerable<Tuple<ICall, JsResolved>> Calls { get { return this.calls; } }

        private Func<ICall, JsResolved> fnCallResolver;
        //private List<Tuple<ICall, JsResolved>> calls = new List<Tuple<ICall, JsResolved>>();

        private ICode VisitCall(ICall call) {
            var resolved = this.fnCallResolver(call);
            if (resolved == null) {
                //this.calls.Add(Tuple.Create(call, (JsResolved)null));
                return null;
            }
            switch (resolved.Type) {
            case JsResolvedType.Expr:
                return ((JsResolvedExpr)resolved).Expr;
            case JsResolvedType.Name:
                //this.calls.Add(Tuple.Create(call, resolved));
                return null;
            default:
                throw new NotImplementedException("Cannot handle: " + resolved.Type);
            }
        }

        protected override ICode VisitCall(ExprCall e) {
            e = base.HandleCall(e, (method, obj, args) => new ExprCall(method, obj, args, e.IsVirtual));
            var res = this.VisitCall((ICall)e);
            if (res == null) {
                return e;
            } else {
                return this.Visit(res);
            }
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            e = this.HandleCall(e, (ctor, obj, args) => new ExprNewObj(ctor, args));
            var res = this.VisitCall((ICall)e);
            if (res == null) {
                return e;
            } else {
                return this.Visit(res);
            }
        }

    }
}
