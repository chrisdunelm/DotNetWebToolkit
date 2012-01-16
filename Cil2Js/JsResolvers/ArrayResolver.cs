using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Output;

namespace Cil2Js.JsResolvers {
    static class ArrayResolver {

        public static Expr Copy(ICall method) {
            // d = arg[0].slice(arg[1],arg[4]+arg[1])
            // Array.prototype.splice.apply(arg[2], [arg[3], arg[4]].concat(d))
            var ctx = method.Ctx;
            var src = method.Args.ElementAt(0);
            var srcIdx = method.Args.ElementAt(1);
            var dst = method.Args.ElementAt(2);
            var dstIdx = method.Args.ElementAt(3);
            var length = method.Args.ElementAt(4);
            var arrayPart = new ExprJsResolvedMethod(ctx, src.Type, src, "slice", srcIdx, ctx.ExprGen.Add(srcIdx, length));
            var spliceFixedArgs = new ExprJsArrayLiteral(ctx, ctx.Object, dstIdx, length);
            var spliceArgs = new ExprJsResolvedMethod(ctx, spliceFixedArgs.Type, spliceFixedArgs, "concat", arrayPart);
            var copy = new ExprJsResolvedMethod(ctx, ctx.Void, null, "Array.prototype.splice.apply", dst, spliceArgs);
            return copy;
        }

    }
}
