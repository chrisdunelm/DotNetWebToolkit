using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Output;
using System.Reflection;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    public static partial class JsResolver {

        private static Type T(string typeName) {
            return Type.GetType(typeName);
        }

        private static Dictionary<Type, Type> reverseTypeMap = map.ToLookup(x => x.Value, x => x.Key).Where(x => x.Count() == 1).ToDictionary(x => x.Key, x => x.First());

        private static readonly ModuleDefinition thisModule = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location);

        private static string JsCase(string s) {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        private static MemberInfo FindJsMember(MethodReference mRef, Type type) {
            Func<IEnumerable<JsAttribute>, string, bool> isMatch = (attrs, mName) => {
                return attrs.Any(a => {
                    if (a.MethodName == null && a.Parameters == null && mName == mRef.Name) {
                        return true;
                    }
                    if (a.MethodName != null) {
                        if (a.MethodName != mRef.Name) {
                            return false;
                        }
                    } else {
                        if (mName != mRef.Name) {
                            return false;
                        }
                    }
                    if (a.Parameters.Count() != mRef.Parameters.Count) {
                        return false;
                    }
                    for (int i = 0; i < mRef.Parameters.Count; i++) {
                        if (a.Parameters.ElementAt(i).FullName != mRef.Parameters[i].ParameterType.FullName) {
                            return false;
                        }
                    }
                    if (a.ReturnType.FullName != mRef.ReturnType.FullName) {
                        return false;
                    }
                    return true;
                });
            };
            var members = type.GetMembers();
            var member = members.FirstOrDefault(m => {
                var attrs = m.GetCustomAttributes<JsAttribute>();
                return isMatch(attrs, m.Name);
            });
            if (member == null && !type.IsDefined(typeof(JsIncompleteAttribute))) {
                var jss = type.GetCustomAttributes<JsAttribute>();
                if (!isMatch(jss, null)) {
                    throw new InvalidOperationException("Cannot find method: " + mRef.FullName);
                }
            }
            return member;
        }

        public static Expr ResolveCall(ICall call) {
            var ctx = call.Ctx;
            var mRef = call.CallMethod;
            var mDef = mRef.Resolve();
            // A call defined on a class requiring external methods/properties translating to native JS
            var type = mDef.DeclaringType;
            var jsClass = type.GetCustomAttribute<JsClassAttribute>();
            if (jsClass != null && mDef.IsExternal()) {
                if (mDef.IsSetter || mDef.IsGetter) {
                    var propertyName = JsCase(mDef.Name.Substring(4));
                    if (mDef.IsStatic) {
                        propertyName = JsCase(mDef.DeclaringType.Name) + "." + propertyName;
                    }
                    return new ExprJsResolvedProperty(ctx, call.Type, call.Obj, propertyName);
                } else {
                    var methodName = JsCase(mDef.Name);
                    if (mDef.IsStatic) {
                        methodName = JsCase(mDef.DeclaringType.Name) + "." + methodName;
                    }
                    return new ExprJsResolvedMethod(ctx, call.Type, call.Obj, methodName, call.Args);
                }
            }
            var callType = Type.GetType(mRef.DeclaringType.Resolve().FullName);
            if (callType == null) {
                // Method is outside this module or its references
                return null;
            }
            var altType = map.ValueOrDefault(callType);
            if (altType != null) {
                // Look for methods in C# that the call can be redirected to
                var tRef = thisModule.Import(altType);
                var mappedMethod = tRef.EnumResolvedMethods().FirstOrDefault(x => x.Resolve().IsPublic && x.MatchMethodOnly(mRef));
                if (mappedMethod != null) {
                    Expr expr;
                    if (mDef.IsConstructor) {
                        expr = new ExprNewObj(ctx, mappedMethod, call.Args);
                    } else {
                        expr = new ExprCall(ctx, mappedMethod, call.Obj, call.Args, call.IsVirtualCall);
                    }
                    return expr;
                }
                // Look for methods that generate AST
                var method = FindJsMember(call.CallMethod, altType);
                if (method != null && method.ReturnType() == typeof(Expr)) {
                    var expr = (Expr)((MethodInfo)method).Invoke(null, new object[] { call });
                    return expr;
                }
            }
            return null;
        }

        public static Stmt ResolveMethod(Ctx ctx) {
            // Attribute for internal function
            var jsAttr = ctx.MDef.GetCustomAttribute<JsAttribute>();
            if (jsAttr != null) {
                var arg0 = jsAttr.ConstructorArguments[0];
                switch (arg0.Type.MetadataType) {
                case MetadataType.String: {
                        var js = (string)arg0.Value;
                        var args = Enumerable.Range(0, ctx.MRef.Parameters.Count).Select(i => ctx[i]).Concat(ctx.MRef.HasThis ? ctx.ThisNamed : null).ToArray();
                        var stmt = new StmtJsExplicit(ctx, js, args);
                        return stmt;
                    }
                default: {
                        var implType = (TypeDefinition)arg0.Value;
                        var t = typeof(JsResolver).Module.ResolveType(implType.MetadataToken.ToInt32());
                        var impl = (IJsImpl)Activator.CreateInstance(t);
                        var stmt = impl.GetImpl(ctx);
                        return stmt;
                    }
                }
            }
            // Type map
            var methodType = Type.GetType(ctx.TRef.Resolve().FullName);
            if (methodType == null) {
                // Method is outside this module or its references
                return null;
            }
            var altType = map.ValueOrDefault(methodType);
            if (altType != null) {
                // Look for methods that generate AST
                var method = FindJsMember(ctx.MRef, altType);
                if (method != null && method.ReturnType() == typeof(Stmt)) {
                    var stmt = (Stmt)((MethodInfo)method).Invoke(null, new object[] { ctx });
                    return stmt;
                }
            }
            return null;
        }

        public static TypeReference ReverseTypeMap(TypeReference tRef) {
            if (tRef.IsNested || tRef.IsGenericInstance) {
                return tRef;
            }
            var type = Type.GetType(tRef.FullName);
            if (type == null) {
                // Generic types cannot be got this way, but that's fine because they never need to be (so far...)
                return tRef;
            }
            var revType = reverseTypeMap.ValueOrDefault(type);
            if (revType == null) {
                return tRef;
            }
            var tRefRev = thisModule.Import(revType);
            return tRefRev;
        }

    }
}
