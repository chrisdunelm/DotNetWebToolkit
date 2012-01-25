using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    [JsIncomplete]
    class _Array {

        [Js("Copy", typeof(void), typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int))]
        public static Expr Copy(ICall call) {
            // d = arg[0].slice(arg[1],arg[4]+arg[1])
            // Array.prototype.splice.apply(arg[2], [arg[3], arg[4]].concat(d))
            var ctx = call.Ctx;
            var src = call.Args.ElementAt(0);
            var srcIdx = call.Args.ElementAt(1);
            var dst = call.Args.ElementAt(2);
            var dstIdx = call.Args.ElementAt(3);
            var length = call.Args.ElementAt(4);
            var arrayPart = new ExprJsResolvedMethod(ctx, src.Type, src, "slice", srcIdx, ctx.ExprGen.Add(srcIdx, length));
            var spliceFixedArgs = new ExprJsArrayLiteral(ctx, ctx.Object, dstIdx, length);
            var spliceArgs = new ExprJsResolvedMethod(ctx, spliceFixedArgs.Type, spliceFixedArgs, "concat", arrayPart);
            var copy = new ExprJsResolvedMethod(ctx, ctx.Void, null, "Array.prototype.splice.apply", dst, spliceArgs);
            return copy;
        }

        [Js]
        public static Stmt Clear(Ctx ctx) {
            var array = ctx.MethodParameter(0);
            var index = ctx.MethodParameter(1);
            var length = ctx.MethodParameter(2);
            var arrayElementType = array.Type.GetElementType();
            var i = new ExprVarLocal(ctx, ctx.Int32);
            var value = new ExprDefaultValue(ctx, arrayElementType);
            var js = "for ({0}=0; {0}<{1}; {0}++) {{ {2}[{3}+{0}]={4}; }}";
            var stmt = new StmtJsExplicit(ctx, js, i, length, array, index, value);
            return stmt;
        }

    }
}
