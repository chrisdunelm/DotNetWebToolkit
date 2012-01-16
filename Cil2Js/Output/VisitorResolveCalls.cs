using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public class VisitorResolveCalls : JsAstVisitor {

        public VisitorResolveCalls(Func<ICall, Expr> fnCallResolver) {
            this.fnCallResolver = fnCallResolver;
        }

        private Func<ICall, Expr> fnCallResolver;

        private ICode VisitCall(ICall call) {
            var resolved = this.fnCallResolver(call);
            return resolved;
        }

        protected override ICode VisitCall(ExprCall e) {
            e = this.HandleCall(e, (method, obj, args) => new ExprCall(e.Ctx, method, obj, args, e.IsVirtualCall));
            var res = this.VisitCall((ICall)e);
            if (res == null) {
                return e;
            } else {
                return this.Visit(res);
            }
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            e = this.HandleCall(e, (ctor, obj, args) => new ExprNewObj(e.Ctx, ctor, args));
            var res = this.VisitCall((ICall)e);
            if (res == null) {
                return e;
            } else {
                return this.Visit(res);
            }
        }

    }
}
