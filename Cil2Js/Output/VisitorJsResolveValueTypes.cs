using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsResolveValueTypes : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveValueTypes();
            return v.Visit(ast);
        }

        private ICode HandleCall(ICall call, Func<Expr, IEnumerable<Expr>, ICode> fnNew) {
            if (call.CallMethod.Name == "DeepCopyValueType") {
                // HACK
                return call;
            }
            var obj = (Expr)this.Visit(call.Obj);
            var argsInfo = call.Args.Select((arg, i) => new { arg, parameterType = call.CallMethod.Parameters[i].ParameterType }).ToArray();
            var args = this.HandleList(argsInfo, argInfo => {
                if (!argInfo.parameterType.IsValueType) {
                    return argInfo;
                }
                var arg = argInfo.arg;
                if (arg.Type.IsNonPrimitiveValueType()) {
                    return new { arg = InternalFunctions.ValueTypeDeepCopyIfRequired(arg.Type, () => arg) ?? arg, parameterType = argInfo.parameterType };
                } else {
                    return argInfo;
                }
            }).NullThru(x => x.Select(y => y.arg).ToArray());
            if (obj != call.Obj || args != null) {
                return fnNew(obj, args ?? call.Args);
            } else {
                return call;
            }
        }

        protected override ICode VisitCall(ExprCall e) {
            return this.HandleCall(e, (obj, args) => new ExprCall(e.Ctx, e.CallMethod, obj, args, e.IsVirtualCall, e.ConstrainedType, e.Type));
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            return this.HandleCall(e, (obj, args) => new ExprNewObj(e.Ctx, e.CallMethod, args));
        }

        protected override ICode VisitJsVirtualCall(ExprJsVirtualCall e) {
            return this.HandleCall(e, (obj, args) => new ExprJsVirtualCall(e.Ctx, e.CallMethod, e.RuntimeType, obj, args));
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            var expr = (Expr)this.Visit(s.Expr);
            var type = expr.Type;
            if (type.IsNonPrimitiveValueType()) {
                if (expr.ExprType != Expr.NodeType.VarThis && !type.IsRuntimeHandle()) {
                    // This will not copy pointers to value-types, which is correct.
                    // It will only copy actual value-types, except 'this' and runtime handles
                    expr = InternalFunctions.ValueTypeDeepCopyIfRequired(type, () => expr) ?? expr;
                }
            }
            if (expr != s.Expr) {
                return new StmtAssignment(s.Ctx, s.Target, expr);
            } else {
                return s;
            }
        }

        protected override ICode VisitAssignment(ExprAssignment e) {
            var expr = (Expr)this.Visit(e.Expr);
            var type = expr.Type;
            if (type.IsNonPrimitiveValueType()) {
                if (expr.ExprType != Expr.NodeType.VarThis && !type.IsRuntimeHandle()) {
                    // This will not copy pointers to value-types, which is correct.
                    // It will only copy actual value-types, except 'this' and runtime handles
                    expr = InternalFunctions.ValueTypeDeepCopyIfRequired(expr.Type, () => expr) ?? expr;
                }
            }
            if (expr != e.Expr) {
                return new StmtAssignment(e.Ctx, e.Target, expr);
            } else {
                return e;
            }
        }

    }
}
