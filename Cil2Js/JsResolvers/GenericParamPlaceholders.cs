using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    //static class GenericParamPlaceholders {

    //    //public static string ResolveFullName(Type type, MethodReference mRef) {
    //    //    if (type.IsArray) {
    //    //        return ResolveFullName(type.GetElementType(), mRef) + "[]";
    //    //    }
    //    //    if (type.IsGenericType) {
    //    //        var argNames = type.GenericTypeArguments.Select(x => ResolveFullName(x, mRef)).ToArray();
    //    //        var genTypeDef = type.GetGenericTypeDefinition();
    //    //        var ret = genTypeDef.FullName + "<" + string.Join(",", argNames) + ">";
    //    //        return ret;
    //    //    }
    //    //    if (type == typeof(GenTypeParam0)) {
    //    //        return ((GenericInstanceType)mRef.DeclaringType).GenericArguments[0].FullName;
    //    //    }
    //    //    if (type == typeof(GenTypeParam1)) {
    //    //        return ((GenericInstanceType)mRef.DeclaringType).GenericArguments[1].FullName;
    //    //    }
    //    //    if (type == typeof(GenTypeParam2)) {
    //    //        return ((GenericInstanceType)mRef.DeclaringType).GenericArguments[2].FullName;
    //    //    }
    //    //    if (type == typeof(GenTypeParam3)) {
    //    //        return ((GenericInstanceType)mRef.DeclaringType).GenericArguments[3].FullName;
    //    //    }
    //    //    if (type == typeof(GenMethodParam0)) {
    //    //        return ((GenericInstanceMethod)mRef).GenericArguments[0].FullName;
    //    //    }
    //    //    if (type == typeof(GenMethodParam1)) {
    //    //        return ((GenericInstanceMethod)mRef).GenericArguments[1].FullName;
    //    //    }
    //    //    if (type == typeof(GenMethodParam2)) {
    //    //        return ((GenericInstanceMethod)mRef).GenericArguments[2].FullName;
    //    //    }
    //    //    if (type == typeof(GenMethodParam3)) {
    //    //        return ((GenericInstanceMethod)mRef).GenericArguments[3].FullName;
    //    //    }
    //    //    return type.FullName;
    //    //}

    //}

    class GenTypeParam0 { }
    class GenTypeParam1 { }
    class GenTypeParam2 { }
    class GenTypeParam3 { }

    class GenMethodParam0 { }
    class GenMethodParam1 { }
    class GenMethodParam2 { }
    class GenMethodParam3 { }

}
