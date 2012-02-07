using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    using Cls = DotNetWebToolkit.Cil2Js.JsResolvers.Classes;

    public static partial class JsResolver {

        private static Dictionary<Type, Type> map = new Dictionary<Type, Type>() {
            { typeof(bool), typeof(Cls._Boolean) },
            { typeof(char), typeof(Cls._Char) },
            { typeof(string), typeof(Cls._String) },
            { typeof(Int64), typeof(Cls._Int64) },
            { typeof(UInt64), typeof(Cls._UInt64) },
            { typeof(double), typeof(Cls._Double) },
            { typeof(StringBuilder), typeof(Cls._StringBuilder) },
            { typeof(object), typeof(Cls._Object) },
            { typeof(Type), typeof(Cls._Type) },
            { T("System.RuntimeType"), typeof(Cls._RuntimeType) },
            { typeof(NumberFormatInfo), typeof(Cls._NumberFormatInfo) },
            { T("System.Number"), typeof(Cls._Number) },
            { T("System.Runtime.CompilerServices.RuntimeHelpers"), typeof(Cls._RuntimeHelpers) },
            { T("System.ThrowHelper"), typeof(Cls._ThrowHelper) },
            { typeof(Environment), typeof(Cls._Environment) },
            { typeof(EqualityComparer<>), typeof(Cls._EqualityComparer<>) },
            { typeof(Math), typeof(Cls._Math) },
            { typeof(Array), typeof(Cls._Array) },
            { typeof(Action), typeof(Cls._Action) },
            { typeof(Action<>), typeof(Cls._Action) },
            { typeof(Action<,>), typeof(Cls._Action) },
            { typeof(Action<,,>), typeof(Cls._Action) },
            { typeof(Action<,,,>), typeof(Cls._Action) },
            { typeof(Action<,,,,>), typeof(Cls._Action) },
            { typeof(Action<,,,,,>), typeof(Cls._Action) },
            { typeof(Action<,,,,,,>), typeof(Cls._Action) },
            { typeof(Action<,,,,,,,>), typeof(Cls._Action) },
            { typeof(Func<>), typeof(Cls._Action) },
            { typeof(Func<,>), typeof(Cls._Action) },
            { typeof(Func<,,>), typeof(Cls._Action) },
            { typeof(Func<,,,>), typeof(Cls._Action) },
            { typeof(Func<,,,,>), typeof(Cls._Action) },
            { typeof(Func<,,,,,>), typeof(Cls._Action) },
            { typeof(Func<,,,,,,>), typeof(Cls._Action) },
            { typeof(Func<,,,,,,,>), typeof(Cls._Action) },
            { typeof(Func<,,,,,,,,>), typeof(Cls._Action) },
        };

    }
}
