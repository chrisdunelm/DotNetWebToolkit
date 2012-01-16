using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {

    public enum JsExprType {
        First = Expr.NodeType.Max,

        JsFunction,
        JsInvoke,
        JsVarMethodReference,
        JsEmptyFunction,
        JsVirtualCall,
        JsArrayLiteral,
        JsResolvedMethod,
        JsResolvedProperty,
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
            case JsExprType.JsVirtualCall:
                return this.VisitJsVirtualCall((ExprJsVirtualCall)e);
            case JsExprType.JsArrayLiteral:
                return this.VisitJsArrayLiteral((ExprJsArrayLiteral)e);
            case JsExprType.JsResolvedMethod:
                return this.VisitJsResolvedMethod((ExprJsResolvedMethod)e);
            case JsExprType.JsResolvedProperty:
                return this.VisitJsResolvedProperty((ExprJsResolvedProperty)e);
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
            var args = this.HandleList(e.Args, x => (Expr)this.Visit(x));
            if (body != e.Body || args != null) {
                return new ExprJsFunction(e.Ctx, args ?? e.Args, body);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitJsInvoke(ExprJsInvoke e) {
            this.ThrowOnNoOverride();
            var methodToInvoke = (Expr)this.Visit(e.MethodToInvoke);
            var args = this.HandleList(e.Args, x => (Expr)this.Visit(x));
            if (methodToInvoke != e.MethodToInvoke || args != null) {
                return new ExprJsInvoke(e.Ctx, methodToInvoke, args ?? e.Args, e.Type);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitJsEmptyFunction(ExprJsEmptyFunction e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitJsVarMethodReference(ExprJsVarMethodReference e) {
            this.ThrowOnNoOverride();
            return e;
        }

        protected virtual ICode VisitJsVirtualCall(ExprJsVirtualCall e) {
            this.ThrowOnNoOverride();
            var objInit = (Expr)this.Visit(e.ObjInit);
            var objRef = (Expr)this.Visit(e.ObjRef);
            var args = this.HandleList(e.Args, x => (Expr)this.Visit(x));
            if (objInit != e.ObjInit || objRef != e.ObjRef || args != null) {
                return new ExprJsVirtualCall(e.Ctx, e.CallMethod, objInit, objRef, args ?? e.Args);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitJsArrayLiteral(ExprJsArrayLiteral e) {
            this.ThrowOnNoOverride();
            var elements = this.HandleList(e.Elements, x => (Expr)this.Visit(x));
            if (elements != null) {
                return new ExprJsArrayLiteral(e.Ctx, e.ElementType, elements);
            } else {
                return e;
            }

        }

        protected virtual ICode VisitJsResolvedMethod(ExprJsResolvedMethod e) {
            this.ThrowOnNoOverride();
            var obj = (Expr)this.Visit(e.Obj);
            var args = this.HandleList(e.Args, x => (Expr)this.Visit(x));
            if (obj != e.Obj || args != null) {
                return new ExprJsResolvedMethod(e.Ctx, e.Type, obj, e.MethodName, args ?? e.Args);
            } else {
                return e;
            }
        }

        protected virtual ICode VisitJsResolvedProperty(ExprJsResolvedProperty e) {
            this.ThrowOnNoOverride();
            var obj = (Expr)this.Visit(e.Obj);
            if (obj != e.Obj) {
                return new ExprJsResolvedProperty(e.Ctx, e.Type, obj, e.PropertyName);
            } else {
                return e;
            }
        }

    }
}
