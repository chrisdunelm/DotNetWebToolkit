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
            var ctx = call.Ctx;
            var array = (ExprVar)call.Args.ElementAt(0);
            var initExpr = (ExprRuntimeHandle)call.Args.ElementAt(1);
            var initData = ((FieldDefinition)initExpr.Member).InitialValue;
            var arrayElType = ((ArrayType)array.Type).ElementType;

            int inc;
            Func<int, object> getValue;
            switch (arrayElType.MetadataType) {
            case MetadataType.Boolean:
                inc = 1;
                getValue = i => initData[i] != 0;
                break;
            case MetadataType.Byte:
                inc = 1;
                getValue = i => initData[i];
                break;
            case MetadataType.SByte:
                inc = 1;
                getValue = i => (sbyte)initData[i];
                break;
            case MetadataType.Int16:
                inc = 2;
                getValue = i => BitConverter.ToInt16(initData, i);
                break;
            case MetadataType.Int32:
                inc = 4;
                getValue = i => BitConverter.ToInt32(initData, i);
                break;
            case MetadataType.Int64:
                inc = 8;
                getValue = i => BitConverter.ToInt64(initData, i);
                break;
            case MetadataType.UInt16:
                inc = 2;
                getValue = i => BitConverter.ToUInt16(initData, i);
                break;
            case MetadataType.UInt32:
                inc = 4;
                getValue = i => BitConverter.ToUInt32(initData, i);
                break;
            case MetadataType.UInt64:
                inc = 8;
                getValue = i => BitConverter.ToUInt64(initData, i);
                break;
            case MetadataType.Single:
                inc = 4;
                getValue = i => BitConverter.ToSingle(initData, i);
                break;
            case MetadataType.Double:
                inc = 8;
                getValue = i => BitConverter.ToDouble(initData, i);
                break;
            case MetadataType.Char:
                inc = 2;
                getValue = i => (char)BitConverter.ToUInt16(initData, i);
                break;
            default:
                throw new NotImplementedException("Cannot handle: " + arrayElType.MetadataType);
            }
            var values = new List<NamedExpr>();
            for (int i = 0; i < initData.Length; i += inc) {
                var value = new ExprLiteral(ctx, getValue(i), arrayElType);
                values.Add(value.Named("v" + i));
            }
            var vStr = string.Join(",", values.Select(x => x.Name));
            var arrayTypeName = new ExprJsTypeVarName(call.Ctx, array.Type).Named("typeName");
            var js = "a = [" + vStr + "]; a._ = typeName";
            return new ExprJsExplicit(call.Ctx, js, array.Type, values.Concat(array.Named("a")).Concat(arrayTypeName));
        }

    }
}
