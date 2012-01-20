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
            var expr = this.HandleCall(e, (method, obj, args) => new ExprCall(e.Ctx, method, obj, args, e.IsVirtualCall));
            var res = JsResolver.ResolveCall(expr);
            if (res != null) {
                return this.Visit(res);
            }
            if (e.IsVirtualCall) {
                var ctx = e.Ctx;
                var objIsVar = e.Obj.IsVar();
                var temp = objIsVar ? null : new ExprVarLocal(ctx, e.Obj.Type);
                var initExpr = objIsVar ? e.Obj : new ExprAssignment(ctx, temp, e.Obj);
                var eJsVCall = new ExprJsVirtualCall(ctx, e.CallMethod, initExpr, temp ?? e.Obj, e.Args);
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

        protected override ICode VisitBox(ExprBox e) {
            if (!e.Type.IsValueType) {
                // For ref-types 'box' does nothing
                return base.Visit(e.Expr);
            }
            var ctx = e.Ctx;
            var eType = new ExprJsTypeVarName(ctx, e.Type);
            if (e.Type.IsNullable()) {
                var genType = (GenericInstanceType)e.Type;
                var innerType = genType.GenericArguments[0];
                var temp = e.Expr.IsVar() ? null : new ExprVarLocal(ctx, e.Type);
                var value = new ExprFieldAccess(ctx, temp ?? e.Expr, e.Type.GetField("value"));
                var dcnExpr = InternalFunctions.ValueTypeDeepCopyIfRequired(innerType, () => value);
                var fHasValue = new ExprJsFieldVarName(ctx, e.Type.GetField("hasValue"));
                var nJs = e.Expr.IsVar() ? "({1}.{2}?{{v:{3},_:{4}}}:null)" : "(({0}={1}).{2}?{{v:{3},_:{4}}}:null)";
                var nExpr = new ExprJsExplicit(ctx, nJs, e.Type, temp, e.Expr, fHasValue, dcnExpr ?? value, eType);
                return nExpr;
            }
            var deepCopyExpr = InternalFunctions.ValueTypeDeepCopyIfRequired(e.Type, () => (Expr)this.Visit(e.Expr));
            var js = "{{v:{0},_:{1}}}";
            var expr = new ExprJsExplicit(ctx, js, e.Type, deepCopyExpr ?? e.Expr, eType);
            return expr;
        }

        protected override ICode VisitUnboxAny(ExprUnboxAny e) {
            if (e.Type.IsValueType) {
                if (e.Type.IsNullable()) {
                    var ctx = e.Ctx;
                    var temp = new ExprVarLocal(ctx, e.Type);
                    var hasValue = new ExprJsFieldVarName(ctx, e.Type.GetField("hasValue"));
                    var value = new ExprJsFieldVarName(ctx, e.Type.GetField("value"));
                    var defaultValue = new ExprDefaultValue(ctx, ((GenericInstanceType)e.Type).GenericArguments[0]);
                    // The Value has to be set even if hasValue==false, as Nullable.GetValueOrDefault() needs it
                    var js = e.Expr.IsVar() ? "{{{2}:!!{1},{3}:{1}||{4}}}" : "{{{2}:!!({0}={1}),{3}:{0}||{4}}}";
                    var expr = new ExprJsExplicit(ctx, js, e.Type, temp, e.Expr, hasValue, value, defaultValue);
                    return expr;
                } else {
                    return base.VisitUnboxAny(e);
                }
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
