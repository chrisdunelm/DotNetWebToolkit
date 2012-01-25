using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _RuntimeHelpers {

        [Js]
        public static Expr InitializeArray(ICall call) {
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
