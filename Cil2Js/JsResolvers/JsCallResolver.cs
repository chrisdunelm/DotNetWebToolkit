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

        private const string TVoid = "System.Void";
        private const string TObject = "System.Object";
        private const string TIntPtr = "System.IntPtr";
        private const string TBoolean = "System.Boolean";
        private const string TString = "System.String";
        private const string TChar = "System.Char";
        private const string TInt32 = "System.Int32";

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

        private static Dictionary<M, Func<ICall, JsResolved>> map = new Dictionary<M, Func<ICall, JsResolved>>(M.ValueEqComparer) {
            { M.Def(TVoid, "System.Action..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`1..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`2..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`3..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`4..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`5..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`6..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`7..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action`8..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Action.Invoke"), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`1.Invoke", (string)null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`2.Invoke", (string)null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`3.Invoke", (string)null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`4.Invoke", (string)null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`5.Invoke", (string)null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`6.Invoke", (string)null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`7.Invoke", (string)null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Action`8.Invoke", (string)null, null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(TVoid, "System.Func`1..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`2..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`3..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`4..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`5..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`6..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`7..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`8..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(TVoid, "System.Func`9..ctor", TObject, TIntPtr), SystemResolver.ActionFunc_ctor },
            { M.Def(null, "System.Func`1.Invoke"), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`2.Invoke", (string)null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`3.Invoke", (string)null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`4.Invoke", (string)null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`5.Invoke", (string)null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`6.Invoke", (string)null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`7.Invoke", (string)null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`8.Invoke", (string)null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },
            { M.Def(null, "System.Func`9.Invoke", (string)null, null, null, null, null, null, null, null), SystemResolver.ActionFunc_Invoke },

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

            { M.Def(TBoolean, "System.Object.Equals", TObject), SystemResolver.ObjectEquals },
        };

        public static JsResolved Resolve(ICall call) {
            var method = call.CallMethod.Resolve();
            // A call that needs translating into a javascript call
            var m = new M(method);
            var fn = map.ValueOrDefault(m);
            if (fn != null) {
                var resolved = fn(call);
                return resolved;
            }
            // A call that is defined in JS only
            var type = method.DeclaringType;
            var jsClass = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "Cil2Js.Attributes.JsClassAttribute");
            if (jsClass != null) {
                if (method.IsExternal()) {
                    if (method.IsSetter || method.IsGetter) {
                        var propertyName = JsCase(method.Name.Substring(4));
                        if (method.IsStatic) {
                            propertyName = JsCase(method.DeclaringType.Name) + "." + propertyName;
                        }
                        return new JsResolvedProperty(call.Obj, propertyName);
                    } else {
                        var methodName = JsCase(method.Name);
                        if (method.IsStatic) {
                            methodName = JsCase(method.DeclaringType.Name) + "." + methodName;
                        }
                        return new JsResolvedMethod(call.Obj, methodName, call.Args);
                    }
                }
            }
            // No special resolution available
            return null;
        }

        private static string JsCase(string s) {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

    }
}
