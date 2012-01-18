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
            var js = "if ({0}) return {1}; throw {2};";
            var miIsAssignableTo = ((Func<Type, Type, bool>)InternalFunctions.IsAssignableTo).Method;
            var mIsAssignableTo = ctx.Module.Import(miIsAssignableTo);
            var miGetType = typeof(object).GetMethod("GetType");
            var mGetType = ctx.Module.Import(miGetType);
            var fromType = new ExprCall(ctx, mGetType, e.Expr);
            var toType = new ExprJsTypeVarName(ctx, e.Type);
            var isAssignableCall = new ExprCall(ctx, mIsAssignableTo, null, fromType, toType);
            var miInvalidCastexCtor = typeof(InvalidCastException).GetConstructor(new[] { typeof(string) });
            var mInvalidCastexCtor = ctx.Module.Import(miInvalidCastexCtor);
            var exMsg = new ExprLiteral(ctx, "Cannot cast...", ctx.String);
            var ex = new ExprNewObj(ctx, mInvalidCastexCtor, new[] { exMsg });
            var body = new StmtJsExplicitFunction(ctx, js, isAssignableCall, e.Expr, ex);
            var fn = new ExprJsFunction(ctx, Enumerable.Empty<Expr>(), body);
            var invoke = new ExprJsInvoke(ctx, fn, Enumerable.Empty<Expr>(), e.Type);
            return invoke;
        }

        protected override ICode VisitIsInst(ExprIsInst e) {
            var ctx = e.Ctx;
            var eExpr = new ExprVarLocal(ctx, e.Expr.Type);
            var eExprAssign = new ExprAssignment(ctx, eExpr, e.Expr);
            var miIsAssignableTo = ((Func<Type, Type, bool>)InternalFunctions.IsAssignableTo).Method;
            var mIsAssignableTo = ctx.Module.Import(miIsAssignableTo);
            var miGetType = typeof(object).GetMethod("GetType");
            var mGetType = ctx.Module.Import(miGetType);
            var fromType = new ExprCall(ctx, mGetType, eExprAssign);
            var toType = new ExprJsTypeVarName(ctx, e.Type);
            var isAssignableCall = new ExprCall(ctx, mIsAssignableTo, null, fromType, toType);
            var eNull = new ExprLiteral(ctx, null, ctx.Object);
            var expr = new ExprTernary(ctx, isAssignableCall, eExpr, eNull);
            return expr;
        }

    }
}
