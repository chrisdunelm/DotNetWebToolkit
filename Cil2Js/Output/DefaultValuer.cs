using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Output {
    public class DefaultValuer {

        public static string Get(TypeReference type) {
            if (!type.IsValueType) {
                return "null";
            }
            if (type.IsPrimitive) {
                switch (type.FullName) {
                case "System.Boolean": return "false";
                case "System.UIntPtr":
                case "System.Int16":
                case "System.Int32": return "0";
                case "System.Char": return @"'\x00'";
                default: throw new NotImplementedException("Cannot handle: " + type.FullName);
                }
            } else {
                throw new NotImplementedException("Cannot handle structs yet");
            }
        }

    }
}
