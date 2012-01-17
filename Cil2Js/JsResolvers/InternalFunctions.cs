using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    class InternalFunctions {

        public static bool IsAssignableTo(Type from, Type to) {
            // Rules from ECMA-335 partition III page 21
            // Rule 7
            if (from.IsArray && to.IsArray) {
                return IsAssignableTo(from.GetElementType(), to.GetElementType());
            }
            // Rules 1, 3 (incomplete, not interfaces) and 4
            var t = from;
            do {
                if (t == to) {
                    return true;
                }
                t = t.BaseType;
            } while (t != null);
            // TODO: Support interfaces
            return false;
        }

    }
}
