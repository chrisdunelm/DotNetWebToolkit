using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    public static partial class JsResolver {

        private static Dictionary<TypeReference, TypeReference> typeMap = new Dictionary<TypeReference, TypeReference>(TypeExtensions.TypeRefEqComparerInstance) {

        };

        public static TypeReference ResolveType(TypeReference type) {
            var mapped = typeMap.ValueOrDefault(type);
            return mapped;
        }

    }
}
