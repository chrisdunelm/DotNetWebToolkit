using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers.Classes;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsResolve64Bit : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolve64Bit();
            return v.Visit(ast);
        }

        protected override ICode VisitUnary(ExprUnary e) {
            if (e.Expr.Type.IsInt64() || e.Expr.Type.IsUInt64()) {
                var ctx = e.Ctx;
                switch (e.Op) {
                case UnaryOp.Negate:
                    var zero = ctx.Literal(0L, ctx.Int64);
                    var subCall = new ExprBinary(ctx, BinaryOp.Sub, ctx.Int64, zero, e.Expr);
                    return subCall;
                default:
                    throw new NotImplementedException("Cannot handle: " + e.Op);
                }
            }
            return base.VisitUnary(e);
        }

        protected override ICode VisitBinary(ExprBinary e) {
            if (e.Left.Type.IsInt64() || e.Left.Type.IsUInt64()) {
                var ctx = e.Ctx;
                var signed = e.Left.Type.IsInt64();
                Delegate d;
                switch (e.Op) {
                case BinaryOp.Add:
                    d = signed ? (Delegate)(Func<Int64, Int64, Int64>)_Int64.Add : (Func<UInt64, UInt64, UInt64>)_UInt64.Add;
                    break;
                case BinaryOp.Sub:
                    d = signed ? (Delegate)(Func<Int64, Int64, Int64>)_Int64.Subtract : (Func<UInt64, UInt64, UInt64>)_UInt64.Subtract;
                    break;
                case BinaryOp.Mul:
                    d = signed ? (Delegate)(Func<Int64, Int64, Int64>)_Int64.Multiply : (Func<UInt64, UInt64, UInt64>)_UInt64.Multiply;
                    break;
                case BinaryOp.Div:
                    d = (Func<Int64, Int64, Int64>)_Int64.Divide;
                    break;
                case BinaryOp.Div_Un:
                    d = (Func<UInt64, UInt64, UInt64>)_UInt64.Divide;
                    break;
                case BinaryOp.Rem:
                    d = (Func<Int64, Int64, Int64>)_Int64.Remainder;
                    break;
                case BinaryOp.Rem_Un:
                    d = (Func<UInt64, UInt64, UInt64>)_UInt64.Remainder;
                    break;
                default:
                    throw new NotImplementedException("Cannot handle: " + e.Op);
                }
                var m = ctx.Module.Import(d.Method);
                var left = (Expr)this.Visit(e.Left);
                var right = (Expr)this.Visit(e.Right);
                var call = new ExprCall(ctx, m, null, left, right);
                return call;
            }
            return base.VisitBinary(e);
        }

    }
}
