using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    static class ResolverArray {

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

        public static Stmt Clear(Ctx ctx, List<TypeReference> newTypesSeen) {
            var m = ctx.MRef;
            var array = new ExprVarParameter(ctx, m.Parameters[0]);
            var index = new ExprVarParameter(ctx, m.Parameters[1]);
            var length = new ExprVarParameter(ctx, m.Parameters[2]);
            var arrayElementType = array.Type.GetElementType();
            var i = new ExprVarLocal(ctx, ctx.Int32);
            var value = new ExprDefaultValue(ctx, arrayElementType);
            var js = "for ({0}=0; {0}<{1}; {0}++) {{ {2}[{3}+{0}]={4}; }}";
            var stmt = new StmtJsExplicitFunction(ctx, js, i, length, array, index, value);
            return stmt;
        }

        public static Expr InitializeArray(ICall call) {
            //var array = ctx.MethodParameter(0);
            //var fieldHandle = ctx.MethodParameter(1);
            var array = (ExprVar)call.Args.ElementAt(0);
            var initExpr = (ExprRuntimeHandle)call.Args.ElementAt(1);
            var initData = ((FieldDefinition)initExpr.Member).InitialValue;
            var arrayElType = array.Type.GetElementType();

            var values = new List<string>();
            if (arrayElType.IsInt32()) {
                for (int i = 0; i < initData.Length; i += 4) {
                    var v = BitConverter.ToInt32(initData, i);
                    values.Add(v.ToString());
                }
            }

            var vStr = string.Join(",", values);
            var arrayTypeName = new ExprJsTypeVarName(call.Ctx, array.Type);
            return new ExprJsExplicit(call.Ctx, "{0}=[" + vStr + "];{0}._={1};", array.Type, array, arrayTypeName);
        }

    }
}
