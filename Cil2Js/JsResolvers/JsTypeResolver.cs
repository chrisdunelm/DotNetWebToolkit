using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Utils;
using Mono.Cecil;
using System.Reflection;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    using Cls = DotNetWebToolkit.Cil2Js.JsResolvers.Classes;

    public static partial class JsResolver {

        private static readonly ModuleDefinition thisModule = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location);

        private static string T<U>() {
            return typeof(U).FullName;
        }

        private static Dictionary<string, string> typeMap = new Dictionary<string, string>() {
            { T<StringBuilder>(), T<Cls.StringBuilder>() },
            { "System.Number", T<Cls.Number>() },
        };
        private static Dictionary<TypeReference, TypeReference> reverseTypeMap = new Dictionary<TypeReference, TypeReference>(TypeExtensions.TypeRefEqComparerInstance);

        //private static Dictionary<MethodReference, MethodReference> methodMap = new Dictionary<MethodReference, MethodReference>(TypeExtensions.MethodRefEqComparerInstance) {
        //};


        public static TypeReference ResolveType(TypeReference type) {
            var mapped = typeMap.ValueOrDefault(type.FullName);
            if (mapped == null) {
                return null;
            }
            var ret = thisModule.GetType(mapped);
            if (!reverseTypeMap.ContainsKey(ret)) {
                reverseTypeMap.Add(ret, type);
            }
            return ret;
        }

        public static TypeReference ReverseTypeMap(TypeReference type) {
            return reverseTypeMap.ValueOrDefault(type, type);
        }

    }
}
