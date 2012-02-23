using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {
    class VisitorJsResolveByRefParameters : JsAstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorJsResolveByRefParameters();
            return v.Visit(ast);
        }

        // TODO: Make these methods share most of the code...

        protected override ICode VisitCall(ExprCall e) {
            var ctx = e.Ctx;
            var byRefs = new List<Tuple<Expr, Expr>>();
            var args = e.CallMethod.Parameters.Zip(e.Args, (p, a) => new { p, a })
                .Select(x => {
                    if (x.p.ParameterType.IsByReference) {
                        var wrapper = ctx.Local(ctx.IntPtr);
                        byRefs.Add(Tuple.Create(x.a, (Expr)wrapper));
                        return wrapper;
                    } else {
                        return x.a;
                    }
                })
                .ToArray();
            if (!args.SequenceEqual(e.Args)) {
                var call = new ExprCall(ctx, e.CallMethod, e.Obj, args, e.IsVirtualCall, e.ConstrainedType, e.Type);
                var resultTemp = ctx.Local(e.Type);
                return new ExprJsByRefWrapper(e.Ctx, call, resultTemp, byRefs);
            }
            return base.VisitCall(e);
        }

        protected override ICode VisitJsVirtualCall(ExprJsVirtualCall e) {
            var ctx = e.Ctx;
            var byRefs = new List<Tuple<Expr,Expr>>();
            var args = e.CallMethod.Parameters.Zip(e.Args, (p, a)=>new {p,a})
                .Select(x=>{
                    if (x.p.ParameterType.IsByReference) {
                        var wrapper = ctx.Local(ctx.IntPtr);
                        byRefs.Add(Tuple.Create(x.a, (Expr)wrapper));
                        return wrapper;
                    }else{
                        return x.a;
                    }
                })
                .ToArray();
            if (!args.SequenceEqual(e.Args)) {
                var call = new ExprJsVirtualCall(ctx, e.CallMethod, e.RuntimeType, e.ObjRef, args);
                var resultTemp = ctx.Local(e.Type);
                return new ExprJsByRefWrapper(e.Ctx, call, resultTemp, byRefs);
            }
            return base.VisitJsVirtualCall(e);
        }

    }
}
