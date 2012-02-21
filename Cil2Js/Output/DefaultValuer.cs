using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class DefaultValuer {

        public static string Get(TypeReference type, Dictionary<FieldReference, string> fieldNames) {
            if (!type.IsValueType) {
                return "null";
            }
            if (type.IsPrimitive) {
                var mdt = type.MetadataType;
                switch (mdt) {
                case MetadataType.Boolean: return "false";
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                case MetadataType.UInt64:
                case MetadataType.Byte:
                case MetadataType.SByte:
                case MetadataType.Single:
                case MetadataType.Double:
                case MetadataType.Char: return "0";
                default: throw new NotImplementedException("Cannot handle: " + mdt);
                }
            } else if (type.Resolve().IsEnum) {
                return "0";
            } else {
                var fields = type.EnumResolvedFields().Where(x => !x.Resolve().IsStatic).ToArray();
                var defaultValue = "{" + string.Join(",",
                    fields.Where(x => fieldNames.ContainsKey(x))
                    .Select(x => fieldNames[x] + ":" + Get(x.FieldType, fieldNames))) + "}";
                return defaultValue;
            }
        }

    }
}
