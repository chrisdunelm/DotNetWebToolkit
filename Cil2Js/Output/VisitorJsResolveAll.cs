using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var expr = this.HandleCall(e, (obj, args) => new ExprCall(e.Ctx, e.CallMethod, obj, args, e.IsVirtualCall, e.ConstrainedType, e.Type));
            //var argDeepCopies = this.HandleList(expr.Args, arg => {
            //    if (arg.IsVar() && arg.Type.IsNonPrimitiveValueType()) {
            //        return InternalFunctions.ValueTypeDeepCopyIfRequired(arg.Type, () => arg);
            //    } else {
            //        return arg;
            //    }
            //});
            //if (argDeepCopies != null) {
            //    expr = new ExprCall(expr.Ctx, expr.CallMethod, expr.Obj, argDeepCopies, expr.IsVirtualCall, expr.ConstrainedType, expr.Type);
            //}
            var res = JsResolver.ResolveCallSite(expr);
            if (res != null) {
                return this.Visit(res);
            }
            if (expr.ConstrainedType != null) {
                if (expr.ConstrainedType.IsValueType) {
                    // Map constrained virtual call to a method on a value-type, to a non-virtual call.
                    // This is important as it prevents having to box the value-type, which is very expensive
                    var impl = expr.ConstrainedType.EnumResolvedMethods().FirstOrDefault(x => x.MatchMethodOnly(expr.CallMethod));
                    if (impl != null) {
                        var constrainedCall = new ExprCall(expr.Ctx, impl, expr.Obj, expr.Args, false, null, expr.Type);
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
            var expr = this.HandleCall(e, (obj, args) => new ExprNewObj(e.Ctx, e.CallMethod, args));
            var res = JsResolver.ResolveCallSite(expr);
            if (res == null) {
                return expr;
            } else {
                return this.Visit(res);
            }
        }

        protected override ICode VisitNewArray(ExprNewArray e) {
            var ctx = e.Ctx;
            MethodInfo caGenDef;
            if (e.ElementType.IsValueType) {
                caGenDef = ((Func<int, int[]>)InternalFunctions.CreateArrayValueType<int>).Method.GetGenericMethodDefinition();
            } else {
                caGenDef = ((Func<int, object[]>)InternalFunctions.CreateArrayRefType<object>).Method.GetGenericMethodDefinition();
            }
            var mCreateArray = ctx.Module.Import(caGenDef).MakeGeneric(e.ElementType);
            var expr = new ExprCall(ctx, mCreateArray, null, e.ExprNumElements);
            return expr;
        }

        protected override ICode VisitBinary(ExprBinary e) {
            var ctx = e.Ctx;
            // Special cases for == and != needed as an empty string is false, not true
            if (e.Op == BinaryOp.Equal) {
                if (e.Left.IsLiteralNull()) {
                    if (e.Right.IsVar() && e.Right.Type.IsValueType && !e.Right.Type.IsNullable()) {
                        return ctx.Literal(false); // Non-nullable Value-type can never be null
                    } else {
                        return new ExprJsExplicit(ctx, "(e == null)", ctx.Boolean, e.Right.Named("e"));
                    }
                }
                if (e.Right.IsLiteralNull()) {
                    if (e.Left.IsVar() && e.Left.Type.IsValueType && !e.Left.Type.IsNullable()) {
                        return ctx.Literal(false); // Non-nullable Value-type can never be null
                    } else {
                        return new ExprJsExplicit(ctx, "(e == null)", ctx.Boolean, e.Left.Named("e"));
                    }
                }
            }
            if (e.Op == BinaryOp.NotEqual || e.Op == BinaryOp.GreaterThan_Un) {
                if (e.Left.IsLiteralNull()) {
                    if (e.Right.IsVar() && e.Right.Type.IsValueType && !e.Right.Type.IsNullable()) {
                        return ctx.Literal(true); // Non-nullable Value-type can never be null
                    } else {
                        return new ExprJsExplicit(ctx, "(e != null)", ctx.Boolean, e.Right.Named("e"));
                    }
                }
                if (e.Right.IsLiteralNull()) {
                    if (e.Left.IsVar() && e.Left.Type.IsValueType && !e.Left.Type.IsNullable()) {
                        return ctx.Literal(true); // Non-nullable Value-type can never be null
                    } else {
                        return new ExprJsExplicit(ctx, "(e != null)", ctx.Boolean, e.Left.Named("e"));
                    }
                }
            }
            return base.VisitBinary(e);
        }

        protected override ICode VisitBox(ExprBox e) {
            if (!e.Type.IsValueType) {
                // For ref-types 'box' does nothing
                return this.Visit(e.Expr);
            }
            var ctx = e.Ctx;
            var eType = new ExprJsTypeVarName(ctx, e.Type);
            if (e.Type.IsNullable()) {
                var exprIsVar = e.Expr.IsVar();
                var innerType = e.Type.GetNullableInnerType();
                var temp = exprIsVar ? null : new ExprVarLocal(ctx, e.Type);
                var box = new ExprBox(ctx, temp ?? e.Expr, innerType).Named("box");
                var nullableJs = exprIsVar ? "(expr !== null ? box : null)" : "((temp = expr) !== null ? box : null)";
                var nullableExpr = new ExprJsExplicit(ctx, nullableJs, innerType, temp.Named("temp"), e.Expr.Named("expr"), box);
                return nullableExpr;
            } else {
                var deepCopyExpr = InternalFunctions.ValueTypeDeepCopyIfRequired(e.Type, () => (Expr)this.Visit(e.Expr));
                var js = "{v:expr,_:type}";
                var expr = new ExprJsExplicit(ctx, js, e.Type, (deepCopyExpr ?? e.Expr).Named("expr"), eType.Named("type"));
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
            var expr = new ExprCall(ctx, e.Type, mCast, null, e.Expr, eType);
            return expr;
        }

        protected override ICode VisitIsInst(ExprIsInst e) {
            var ctx = e.Ctx;
            var mIsInst = ctx.Module.Import(((Func<object, Type, object>)InternalFunctions.IsInst).Method);
            var eType = new ExprJsTypeVarName(ctx, e.Type);
            var expr = new ExprCall(ctx, e.Type, mIsInst, null, e.Expr, eType);
            return expr;
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            var ctx = s.Ctx;
            var expr = (Expr)this.Visit(s.Expr);
            var target = (ExprVar)this.Visit(s.Target);
            if (target.Type.IsBoolean() && expr.Type.IsInteger()) {
                expr = new ExprJsExplicit(ctx, "!!expr", ctx.Boolean, expr.Named("expr"));
            }
            //if (expr.Type.IsNonPrimitiveValueType() && expr.IsVar()) {
            //    expr = InternalFunctions.ValueTypeDeepCopyIfRequired(expr.Type, () => expr) ?? expr;
            //}
            if (expr != s.Expr || target != s.Target) {
                return new StmtAssignment(ctx, target, expr);
            } else {
                return s;
            }
        }

        protected override ICode VisitAssignment(ExprAssignment e) {
            var ctx = e.Ctx;
            var expr = (Expr)this.Visit(e.Expr);
            var target = (ExprVar)this.Visit(e.Target);
            if (target.Type.IsBoolean() && expr.Type.IsInteger()) {
                expr = new ExprJsExplicit(ctx, "!!expr", ctx.Boolean, expr.Named("expr"));
            }
            if (expr != e.Expr || target != e.Target) {
                return new ExprAssignment(ctx, target, expr);
            } else {
                return e;
            }
        }

        protected override ICode VisitReturn(StmtReturn s) {
            var ctx = s.Ctx;
            var expr = (Expr)this.Visit(s.Expr);
            if (expr != null && expr.Type.IsInteger() && ctx.MRef.ReturnType.FullResolve(ctx).IsBoolean()) {
                expr = new ExprJsExplicit(ctx, "!!expr", ctx.Boolean, expr.Named("expr"));
            }
            if (expr != s.Expr) {
                return new StmtReturn(ctx, expr);
            } else {
                return s;
            }
        }

    }
}
