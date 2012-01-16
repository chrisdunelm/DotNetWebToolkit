using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    static class ArrayResolver {

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

        public static Stmt Clear(Ctx ctx) {
            var m = ctx.MRef;
            var array = new ExprVarParameter(ctx, m.Parameters[0]);
            var index = new ExprVarParameter(ctx, m.Parameters[1]);
            var length = new ExprVarParameter(ctx, m.Parameters[2]);
            var arrayElementType = array.Type.GetElementType();
            var i = new ExprVarLocal(ctx, ctx.Int32);
            var b = new ExprVarLocal(ctx, ctx.Boolean);
            var stmt = new StmtBlock(ctx,
                new StmtAssignment(ctx, i, new ExprLiteral(ctx, 0, ctx.Int32)),
                new StmtDoLoop(ctx,
                    new StmtBlock(ctx,
                        new StmtAssignment(ctx, b, new ExprBinary(ctx, BinaryOp.LessThan, ctx.Boolean, i, length)),
                        new StmtIf(ctx, b,
                            new StmtBlock(ctx,
                                new StmtAssignment(ctx,
                                    new ExprVarArrayAccess(ctx, array, new ExprBinary(ctx, BinaryOp.Add, ctx.Int32, i, index)),
                                    new ExprDefaultValue(ctx, arrayElementType)),
                                new StmtAssignment(ctx, i, new ExprBinary(ctx, BinaryOp.Add, ctx.Int32, i, new ExprLiteral(ctx, 1, ctx.Int32)))
                                ),
                            null)
                        ),
                    b)
                );
            return stmt;
        }

    }
}
