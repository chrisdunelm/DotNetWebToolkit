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

        private static Dictionary<M, string> named = new Dictionary<M, string>(M.ValueEqComparer) {
        };

        private static Dictionary<M, Func<Expr.Gen, ICall, Expr>> expred = new Dictionary<M, Func<Expr.Gen, ICall, Expr>>(M.ValueEqComparer) {
            { M.Def(TBoolean, "System.String.op_Equality", TString, TString), StringResolver.op_Equality }
        };

        public static JsResolved Resolve(Expr.Gen exprGen, ICall call) {
            var m = new M(call.CallMethod);
            var exprFn = expred.ValueOrDefault(m);
            if (exprFn != null) {
                var expr = exprFn(exprGen, call);
                if (expr != null) {
                    return new JsResolvedExpr(expr);
                }
            }
            var name = named.ValueOrDefault(m);
            if (name != null) {
                return new JsResolvedName(name);
            }
            // No special resolution available
            return null;
        }

    }
}
