using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public class VisitorResolveFieldAccess : AstVisitor {

        public VisitorResolveFieldAccess(Func<ExprFieldAccess, JsResolved> fnFieldAccessResolver) {
            this.fnFieldAccessResolver = fnFieldAccessResolver;
        }

        private Func<ExprFieldAccess, JsResolved> fnFieldAccessResolver;

        protected override ICode VisitFieldAccess(ExprFieldAccess e) {
            var obj = (Expr)this.Visit(e.Obj);
            if (obj != e.Obj) {
                e = new ExprFieldAccess(e.Ctx, obj, e.Field);
            }
            var resolved = this.fnFieldAccessResolver(e);
            if (resolved == null) {
                return e;
            }
            switch (resolved.Type) {
            case JsResolvedType.Expr:
                var expr = ((JsResolvedExpr)resolved).Expr;
                return this.Visit(expr);
            default:
                throw new NotImplementedException("Cannot handle: " + resolved.Type);
            }
        }

    }

}
