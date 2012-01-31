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
            int inc;
            Func<int, string> getValue;
            switch (arrayElType.MetadataType) {
            case MetadataType.Byte:
                inc = 1;
                getValue = i => initData[i].ToString();
                break;
            case MetadataType.SByte:
                inc = 1;
                getValue = i => ((sbyte)initData[i]).ToString();
                break;
            case MetadataType.Int16:
                inc = 2;
                getValue = i => BitConverter.ToInt16(initData, i).ToString();
                break;
            case MetadataType.Int32:
                inc = 4;
                getValue = i => BitConverter.ToInt32(initData, i).ToString();
                break;
            case MetadataType.Int64:
                inc = 8;
                getValue = i => BitConverter.ToInt64(initData, i).ToString();
                break;
            case MetadataType.UInt16:
                inc = 2;
                getValue = i => BitConverter.ToUInt16(initData, i).ToString();
                break;
            case MetadataType.UInt32:
                inc = 4;
                getValue = i => BitConverter.ToUInt32(initData, i).ToString();
                break;
            case MetadataType.UInt64:
                inc = 8;
                getValue = i => BitConverter.ToUInt64(initData, i).ToString();
                break;
            case MetadataType.Single:
                inc = 4;
                getValue = i => BitConverter.ToSingle(initData, i).ToString();
                break;
            case MetadataType.Double:
                inc = 8;
                getValue = i => BitConverter.ToDouble(initData, i).ToString();
                break;
            default:
                throw new NotImplementedException("Cannot handle: " + arrayElType.MetadataType);
            }

            for (int i = 0; i < initData.Length; i += inc) {
                values.Add(getValue(i));
            }
            var vStr = string.Join(",", values);
            var arrayTypeName = new ExprJsTypeVarName(call.Ctx, array.Type);
            return new ExprJsExplicit(call.Ctx, "{0}=[" + vStr + "];{0}._={1}", array.Type, array, arrayTypeName);
        }

    }
}
