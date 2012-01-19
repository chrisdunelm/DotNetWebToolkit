using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {

    public class VisitorJsResolveAll : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveAll();
            return v.Visit(ast);
        }

        protected override ICode VisitCall(ExprCall e) {
            var expr = this.HandleCall(e, (method, obj, args) => new ExprCall(e.Ctx, method, obj, args, e.IsVirtualCall));
            var res = JsResolver.ResolveCall(expr);
            if (res == null) {
                return expr;
            } else {
                return this.Visit(res);
            }
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var expr = this.HandleCall(e, (ctor, obj, args) => new ExprNewObj(e.Ctx, ctor, args));
            var res = JsResolver.ResolveCall(expr);
            if (res == null) {
                return expr;
            } else {
                return this.Visit(res);
            }
        }

        protected override ICode VisitNewArray(ExprNewArray e) {
            var ctx = e.Ctx;
            var miCreateMethod = typeof(InternalFunctions).GetMethod("CreateArray");
            var mCreateMethod = ctx.Module.Import(miCreateMethod).MakeGeneric(e.ElementType);
            var expr = new ExprCall(ctx, mCreateMethod, null, e.ExprNumElements);
            return expr;
        }

        protected override ICode VisitBinary(ExprBinary e) {
            if (e.Op == BinaryOp.GreaterThan && !e.Left.Type.IsValueType && !e.Right.Type.IsValueType) {
                // C# compiles <obj> != null to <obj> > null. Change to inequality
                return new ExprBinary(e.Ctx, BinaryOp.NotEqual, e.Ctx.Boolean, e.Left, e.Right);
            }
            return base.VisitBinary(e);
        }

        protected override ICode VisitUnbox(ExprUnboxAny e) {
            if (e.Type.IsValueType) {
                return base.VisitUnbox(e);
            } else {
                // On ref-type, unbox-any becomes a castclass
                var expr = (Expr)this.Visit(e.Expr);
                var cast = new ExprCast(e.Ctx, expr, e.Type);
                return cast;
            }
        }

        protected override ICode VisitCast(ExprCast e) {
            var ctx = e.Ctx;
            var mCast = ctx.Module.Import(((Func<object, Type, object>)InternalFunctions.Cast).Method);
            var eType = new ExprJsTypeVarName(ctx, e.Type);
            var expr = new ExprCall(ctx, mCast, null, e.Expr, eType);
            return expr;
        }

        protected override ICode VisitIsInst(ExprIsInst e) {
            var ctx = e.Ctx;
            var mIsInst = ctx.Module.Import(((Func<object, Type, object>)InternalFunctions.IsInst).Method);
            var eType = new ExprJsTypeVarName(ctx, e.Type);
            var expr = new ExprCall(ctx, mIsInst, null, e.Expr, eType);
            return expr;
        }

    }
}
