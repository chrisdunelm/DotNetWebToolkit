using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Output;
using Mono.Cecil;
using Cil2Js.Ast;
using Cil2Js.Utils;

namespace Cil2Js.JsResolvers {
    public static class JsCallResolver {

        private const string TBoolean = "System.Boolean";
        private const string TString = "System.String";
        private const string TChar = "System.Char";
        private const string TInt32 = "System.Int32";

        private static string ArrayOf(string type) {
            return type+"[]";
        }

        class M {

            class ValueEqualityComparer : IEqualityComparer<M> {
                public bool Equals(M x, M y) {
                    return
                        x.ReturnType == y.ReturnType &&
                        x.FullName == y.FullName &&
                        Enumerable.SequenceEqual(x.GenericArgTypes, y.GenericArgTypes) &&
                        Enumerable.SequenceEqual(x.ArgTypes, y.ArgTypes);
                }

                public int GetHashCode(M o) {
                    return o.ReturnType.GetHashCode() ^ o.FullName.GetHashCode();
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

        private static Dictionary<M, Func<Expr.Gen, ICall, JsResolved>> map = new Dictionary<M, Func<Expr.Gen, ICall, JsResolved>>(M.ValueEqComparer) {
            { M.Def(TBoolean, "System.String.op_Equality", TString, TString), StringResolver.op_Equality },
            { M.Def(TInt32, "System.String.get_Length"), StringResolver.get_Length },
            { M.Def(TChar, "System.String.get_Chars", TInt32), StringResolver.get_Chars },
            { M.Def(TString, "System.String.Concat", TString, TString), StringResolver.ConcatStrings },
            { M.Def(TString, "System.String.Concat", TString, TString, TString), StringResolver.ConcatStrings },
            { M.Def(TString, "System.String.Concat", TString, TString, TString, TString), StringResolver.ConcatStrings },
            { M.Def(TString, "System.String.Concat", ArrayOf(TString)), StringResolver.ConcatStringsMany },
            { M.Def(TInt32, "System.String.IndexOf", TChar), StringResolver.IndexOf },
            { M.Def(TInt32, "System.String.IndexOf", TString), StringResolver.IndexOf },
            { M.Def(TInt32, "System.String.IndexOf", TChar, TInt32), StringResolver.IndexOf },
            { M.Def(TInt32, "System.String.IndexOf", TString, TInt32), StringResolver.IndexOf },
            { M.Def(TString, "System.String.Substring", TInt32), StringResolver.Substring },
            { M.Def(TString, "System.String.Substring", TInt32, TInt32), StringResolver.Substring },
        };

        public static JsResolved Resolve(Expr.Gen exprGen, ICall call) {
            var m = new M(call.CallMethod);
            var fn = map.ValueOrDefault(m);
            if (fn != null) {
                var resolved = fn(exprGen, call);
                return resolved;
            }
            // No special resolution available
            return null;
        }

    }
}
