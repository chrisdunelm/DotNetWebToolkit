using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {

    public class VisitorJsResolveAll : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveAll();
            return v.Visit(ast);
        }

        protected override ICode VisitCall(ExprCall e) {
            var expr = this.HandleCall(e, (method, obj, args) => new ExprCall(e.Ctx, method, obj, args, e.IsVirtualCall, e.ConstrainedType));
            var res = JsResolver.ResolveCall(expr);
            if (res != null) {
                return this.Visit(res);
            }
            if (expr.ConstrainedType != null) {
                if (expr.ConstrainedType.IsValueType) {
                    // Map constrained virtual call to a method on a value-type, to a non-virtual call.
                    // This is important as it prevents having to box the value-type, which is very expensive
                    var impl = expr.ConstrainedType.EnumResolvedMethods().FirstOrDefault(x => x.MatchMethodOnly(expr.CallMethod));
                    if (impl != null) {
                        var constrainedCall = new ExprCall(expr.Ctx, impl, expr.Obj, expr.Args, false, null);
                        return constrainedCall;
                    } else {
                        throw new Exception();
                    }
                }
            }
            if (expr.IsVirtualCall) {
                var ctx = expr.Ctx;
                var objIsVar = expr.Obj.IsVar();
                var temp = objIsVar ? null : new ExprVarLocal(ctx, expr.Obj.Type);
                var getTypeObj = objIsVar ? expr.Obj : new ExprAssignment(ctx, temp, expr.Obj);
                var getType = new ExprCall(ctx, typeof(object).GetMethod("GetType"), getTypeObj);
                var eJsVCall = new ExprJsVirtualCall(ctx, expr.CallMethod, getType, temp ?? expr.Obj, expr.Args);
                return eJsVCall;
            }
            return expr;
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
            var caGenDef = ((Func<int, int[]>)InternalFunctions.CreateArray<int>).Method.GetGenericMethodDefinition();
            var mCreateArray = ctx.Module.Import(caGenDef).MakeGeneric(e.ElementType);
            var expr = new ExprCall(ctx, mCreateArray, null, e.ExprNumElements);
            return expr;
        }

        protected override ICode VisitBinary(ExprBinary e) {
            if (e.Op == BinaryOp.GreaterThan && !e.Left.Type.IsValueType && !e.Right.Type.IsValueType) {
                // C# compiles <obj> != null to <obj> > null. Change to inequality
                return new ExprBinary(e.Ctx, BinaryOp.NotEqual, e.Ctx.Boolean, e.Left, e.Right);
            }
            return base.VisitBinary(e);
        }

        protected override ICode VisitBox(ExprBox e) {
            if (!e.Type.IsValueType) {
                // For ref-types 'box' does nothing
                return base.Visit(e.Expr);
            }
            var ctx = e.Ctx;
            var eType = new ExprJsTypeVarName(ctx, e.Type);
            if (e.Type.IsNullable()) {
                var exprIsVar = e.Expr.IsVar();
                var innerType = e.Type.GetNullableInnerType();
                var innerTypeName = new ExprJsTypeVarName(ctx, innerType);
                var temp = exprIsVar ? null : new ExprVarLocal(ctx, e.Type);
                var fHasValue = new ExprJsFieldVarName(ctx, e.Type.GetField("hasValue"));
                var box = new ExprBox(ctx, new ExprFieldAccess(ctx, temp ?? e.Expr, e.Type.GetField("value")), innerType);
                var nullableJs = exprIsVar ? "({1}.{2}?{3}:null)" : "(({0}={1}).{2}?{3}:null)";
                var nullableExpr = new ExprJsExplicit(ctx, nullableJs, innerType, temp, e.Expr, fHasValue, box);
                return nullableExpr;
            } else {
                var deepCopyExpr = InternalFunctions.ValueTypeDeepCopyIfRequired(e.Type, () => (Expr)this.Visit(e.Expr));
                var js = "{{v:{0},_:{1}}}";
                var expr = new ExprJsExplicit(ctx, js, e.Type, deepCopyExpr ?? e.Expr, eType);
                return expr;
            }
        }

        protected override ICode VisitUnboxAny(ExprUnboxAny e) {
            if (!e.Type.IsValueType) {
                // On ref-type, unbox-any becomes a castclass
                var expr = (Expr)this.Visit(e.Expr);
                var cast = new ExprCast(e.Ctx, expr, e.Type);
                return cast;
            }
            var ctx = e.Ctx;
            if (e.Type.IsNullable()) {
                // If obj==null create Nullable with hasValue=false
                // If obj.Type not assignable to e.InnerType throw InvalidCastEx
                var innerType = e.Type.GetNullableInnerType();
                var unboxMethod = ((Func<object, int?>)InternalFunctions.UnboxAnyNullable<int>).Method.GetGenericMethodDefinition();
                var mUnbox = ctx.Module.Import(unboxMethod).MakeGeneric(innerType);
                var unboxAnyCall = new ExprCall(ctx, mUnbox, null, e.Expr);
                return unboxAnyCall;
            } else {
                // If obj==null throw NullRefEx
                // If obj.Type not assignable to e.Type throw InvalidCastEx
                // otherwise unbox
                var unboxMethod = ((Func<object, int>)InternalFunctions.UnboxAnyNonNullable<int>).Method.GetGenericMethodDefinition();
                var mUnbox = ctx.Module.Import(unboxMethod).MakeGeneric(e.Type);
                var unboxAnyCall = new ExprCall(ctx, mUnbox, null, e.Expr);
                return unboxAnyCall;
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
