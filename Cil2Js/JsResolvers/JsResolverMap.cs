using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    using Cls = DotNetWebToolkit.Cil2Js.JsResolvers.Classes;
using DotNetWebToolkit.Web;

    public static partial class JsResolver {

        private static Dictionary<Type, Type> map = new Dictionary<Type, Type>() {
            { typeof(object), typeof(Cls._Object) },
            { typeof(bool), typeof(Cls._Boolean) },
            { typeof(char), typeof(Cls._Char) },
            { typeof(string), typeof(Cls._String) },
            { typeof(Int32), typeof(Cls._Int32) },
            { typeof(UInt32), typeof(Cls._UInt32) },
            { typeof(Int64), typeof(Cls._Int64) },
            { typeof(UInt64), typeof(Cls._UInt64) },
            { typeof(Double), typeof(Cls._Double) },
            { typeof(Single), typeof(Cls._Single) },
            { typeof(Nullable<>), typeof(Cls._Nullable<>) },
            { typeof(StringBuilder), typeof(Cls._StringBuilder) },
            { typeof(Type), typeof(Cls._Type) },
            { T("System.RuntimeType"), typeof(Cls._RuntimeType) },
            { typeof(NumberFormatInfo), typeof(Cls._NumberFormatInfo) },
            { T("System.Runtime.CompilerServices.RuntimeHelpers"), typeof(Cls._RuntimeHelpers) },
            { T("System.ThrowHelper"), typeof(Cls._ThrowHelper) },
            { typeof(Environment), typeof(Cls._Environment) },
            { typeof(Comparer<>), typeof(Cls._Comparer<>) },
            { typeof(EqualityComparer<>), typeof(Cls._EqualityComparer<>) },
            { typeof(Math), typeof(Cls._Math) },
            { typeof(Array), typeof(Cls._Array) },
            { typeof(List<>), typeof(Cls._List<>) },
            { typeof(Dictionary<,>), typeof(Cls._Dictionary<,>) },
            { typeof(HashSet<>), typeof(Cls._HashSet<>) },
            { typeof(Thread), typeof(Cls._Thread) },
            { typeof(Enumerable), typeof(Cls._Enumerable) },
            { typeof(Console), typeof(Cls._Console) },
            { typeof(CultureInfo), typeof(Cls._CultureInfo) },
            { typeof(XMLHttpRequestHelper), typeof(Cls._XMLHttpRequestHelper) },
        };

    }
}
