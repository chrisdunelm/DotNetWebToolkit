using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorRemoveCasts : AstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorRemoveCasts();
            return v.Visit(ast);
        }

        private VisitorRemoveCasts() { }

        private Expr Convert(Expr e, TypeReference toType) {
            if (e == null) {
                return null;
            }
            var eType = e.Type.FullResolve(e.Ctx);
            if (eType.IsAssignableTo(toType)) {
                return e;
            }
            if (e.ExprType == Expr.NodeType.Literal) {
                var eLit = (ExprLiteral)e;
                if (toType.IsChar()) {
                    if (eType.IsInt32()) {
                        return new ExprLiteral(e.Ctx, (char)(int)eLit.Value, e.Ctx.Char);
                    }
                }
                if (toType.IsBoolean()) {
                    if (eType.IsInt32()) {
                        return new ExprLiteral(e.Ctx, ((int)eLit.Value) != 0, e.Ctx.Boolean);
                    }
                }
            }
            return e;
        }

        protected override ICode VisitCall(ExprCall e) {
            var obj = this.Convert(e.Obj, e.CallMethod.DeclaringType);
            var args = e.Args.Select((x, i) => this.Convert(x, e.CallMethod.Parameters[i].ParameterType.FullResolve(e.CallMethod))).ToArray();
            return new ExprCall(e.Ctx, e.CallMethod, obj, args, e.IsVirtualCall);
        }

        protected override ICode VisitNewObj(ExprNewObj e) {
            var args = e.Args.Select((x, i) => this.Convert(x, e.CallMethod.Parameters[i].ParameterType.FullResolve(e.CallMethod))).ToArray();
            return new ExprNewObj(e.Ctx, e.CallMethod, args);
        }

    }
}
