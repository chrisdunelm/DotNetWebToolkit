using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.JsResolvers {
    public static partial class JsResolver {

        private const string TVoid = "System.Void";
        private const string TObject = "System.Object";
        private const string TIntPtr = "System.IntPtr";
        private const string TBoolean = "System.Boolean";
        private const string TString = "System.String";
        private const string TChar = "System.Char";
        private const string TInt32 = "System.Int32";
        private const string TArray = "System.Array";

        private static string ArrayOf(string type) {
            return type + "[]";
        }

        class M {

            class NullIsWildcardEqualityComparer : IEqualityComparer<string> {

                public static readonly IEqualityComparer<string> Instance = new NullIsWildcardEqualityComparer();

                public bool Equals(string x, string y) {
                    return x == null || y == null || x == y;
                }

                public int GetHashCode(string obj) {
                    return 0;
                }
            }

            class ValueEqualityComparer : IEqualityComparer<M> {
                public bool Equals(M x, M y) {
                    return
                        (x.ReturnType == null || y.ReturnType == null || x.ReturnType == y.ReturnType) &&
                        x.FullName == y.FullName &&
                        Enumerable.SequenceEqual(x.GenericArgTypes, y.GenericArgTypes, NullIsWildcardEqualityComparer.Instance) &&
                        Enumerable.SequenceEqual(x.ArgTypes, y.ArgTypes, NullIsWildcardEqualityComparer.Instance);
                }

                public int GetHashCode(M o) {
                    return o.FullName.GetHashCode();
                }
            }

            public static readonly IEqualityComparer<M> ValueEqComparer = new ValueEqualityComparer();

            public static M Def(string returnType, string fullName, params string[] argTypes) {
                return new M(returnType, fullName, Enumerable.Empty<string>(), argTypes);
            }

            public static M Def(string returnType, string fullName, string[] genericArgTypes, params string[] argTypes) {
                return new M(returnType, fullName, genericArgTypes, argTypes);
            }

            private M(string returnType, string fullName, IEnumerable<string> genericArgTypes, IEnumerable<string> argTypes) {
                this.ReturnType = returnType;
                this.FullName = fullName;
                this.GenericArgTypes = genericArgTypes;
                this.ArgTypes = argTypes;
            }

            public M(MethodReference method) {
                this.ReturnType = method.ReturnType.FullName;
                this.FullName = method.DeclaringType.FullName + "." + method.Name;
                this.GenericArgTypes = method.GenericParameters.Select(x => x.FullName).ToArray();
                this.ArgTypes = method.Parameters.Select(x => x.ParameterType.FullName).ToArray();
            }

            public string ReturnType { get; private set; }
            public string FullName { get; private set; }
            public IEnumerable<string> GenericArgTypes { get; private set; }
            public IEnumerable<string> ArgTypes { get; private set; }

        }

        private static string JsCase(string s) {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

    }
}
