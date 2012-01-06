using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Analysis;
using Cil2Js.Ast;

namespace Cil2Js.Output {

    public enum JsExprType {
        First = Expr.NodeType.Max,

        JsFunction,
        JsInvoke,
        JsVarMethodReference,
        JsEmptyFunction,
    }

    public class JsAstVisitor : AstVisitor {

        public JsAstVisitor() : this(false) { }

        public JsAstVisitor(bool throwOnNoOverride)
            : base(throwOnNoOverride) {
        }

        protected override ICode VisitExpr(Expr e) {
            var jsExprType = (JsExprType)e.ExprType;
            switch (jsExprType) {
            case JsExprType.JsFunction:
                return this.VisitJsFunction((ExprJsFunction)e);
            case JsExprType.JsInvoke:
                return this.VisitJsInvoke((ExprJsInvoke)e);
            case JsExprType.JsEmptyFunction:
                return this.VisitJsEmptyFunction((ExprJsEmptyFunction)e);
            case JsExprType.JsVarMethodReference:
                return this.VisitJsVarMethodReference((ExprJsVarMethodReference)e);
            default:
                if ((int)jsExprType >= (int)JsExprType.First) {
                    throw new NotImplementedException("Cannot handle: " + jsExprType);
                } else {
                    return base.VisitExpr(e);
                }
            }
        }

        protected virtual ICode VisitJsFunction(ExprJsFunction e) {
            this.ThrowOnNoOverride();
            var body = (Stmt)this.Visit(e.Body);
            foreach (var arg in e.Args) {
                this.Visit(arg);
            }
            // TODO: Handle properly
            if (body != e.Body) {
                return new ExprJsFunction(e.Ctx, e.Args, body);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitJsInvoke(ExprJsInvoke e) {
            this.ThrowOnNoOverride();
            this.Visit(e.MethodToInvoke);
            foreach (var arg in e.Args) {
                this.Visit(arg);
            }
            // TODO: Handle properly
            return e;
        }

        protected virtual ICode VisitJsEmptyFunction(ExprJsEmptyFunction e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitJsVarMethodReference(ExprJsVarMethodReference e) {
            this.ThrowOnNoOverride();
            return e;
        }

    }
}
