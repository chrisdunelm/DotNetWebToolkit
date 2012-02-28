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

    using Cls = DotNetWebToolkit.Cil2Js.JsResolvers.Classes;

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
                    if (a.IsStaticFull != null) {
                        if (a.IsStaticFull.Value == mRef.HasThis) {
                            return false;
                        }
                    }
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
                        var parameter = mRef.Parameters[i];
                        var mRefParameterResolved = parameter.ParameterType.FullResolve(mRef);
                        var aParameterFullName = Cls.GenericParamPlaceholders.ResolveFullName(a.Parameters.ElementAt(i), mRef);
                        if (aParameterFullName != mRefParameterResolved.FullName) {
                            return false;
                        }
                    }
                    var mRefReturnTypeResolved = mRef.ReturnType.FullResolve(mRef);
                    var aReturnTypeFullName = Cls.GenericParamPlaceholders.ResolveFullName(a.ReturnType, mRef);
                    if (mRefReturnTypeResolved.FullName != aReturnTypeFullName) {
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

        private static TypeReference ImportType(Type type, TypeReference declaringType) {
            var tRef = thisModule.Import(type);
            if (tRef.HasGenericParameters) {
                var args = ((GenericInstanceType)declaringType).GenericArguments.ToArray();
                tRef = tRef.MakeGeneric(args);
            }
            return tRef;
        }

        public static string JsName(MemberReference m) {
            var mType = m.MetadataToken.TokenType;
            string name = JsCase(m.Name);
            CustomAttribute jsDetail = null;
            switch (mType) {
            case TokenType.Field:
                jsDetail = ((FieldDefinition)m).GetCustomAttribute<JsDetailAttribute>();
                break;
            case TokenType.Method:
                jsDetail = ((MethodReference)m).Resolve().GetCustomAttribute<JsDetailAttribute>();
                break;
            default:
                throw new NotImplementedException("Cannot handle: " + mType);
            }
            if (jsDetail != null) {
                var nameProp = jsDetail.Properties.FirstOrDefault(x => x.Name == "Name");
                if (nameProp.Name != null) {
                    name = (string)nameProp.Argument.Value;
                }
            }
            return name;
        }

        public static Expr ResolveCall(ICall call) {
            var ctx = call.Ctx;
            var mRef = call.CallMethod;
            var mDef = mRef.Resolve();
            // A call defined on a class requiring external methods/properties translating to native JS
            var type = mDef.DeclaringType;
            var jsClass = type.GetCustomAttribute<JsClassAttribute>() ?? type.GetCustomAttribute<JsAbstractClassAttribute>();
            if (jsClass != null && mDef.IsExternal()) {
                var jsDetail = mDef.GetCustomAttribute<JsDetailAttribute>();
                if (mDef.IsSetter || mDef.IsGetter) {
                    // TODO: Use JsName()
                    var property = mDef.DeclaringType.Properties.First(x => {
                        if (x.GetMethod != null && TypeExtensions.MethodRefEqComparerInstance.Equals(x.GetMethod, mDef)) {
                            return true;
                        }
                        if (x.SetMethod != null && TypeExtensions.MethodRefEqComparerInstance.Equals(x.SetMethod, mDef)) {
                            return true;
                        }
                        return false;
                    });
                    jsDetail = property.GetCustomAttribute<JsDetailAttribute>();
                    string propertyName = null;
                    if (jsDetail != null) {
                        var isDomEventProp = jsDetail.Properties.FirstOrDefault(x => x.Name == "IsDomEvent");
                        if (isDomEventProp.Name != null && (bool)isDomEventProp.Argument.Value) {
                            // Special handling of DOM events
                            if (!mDef.Name.StartsWith("set_On")) {
                                throw new InvalidOperationException("DOM event name must start with 'On'");
                            }
                            if (!mDef.IsSetter) {
                                throw new InvalidOperationException("Only setters supported on DOM events");
                            }
                            if (call.Args.Count() != 1) {
                                throw new InvalidOperationException("DOM event setter should have one argument");
                            }
                            var eventName = mDef.Name.Substring(6).ToLowerInvariant();
                            var eventNameExpr = ctx.Literal(eventName, ctx.String);
                            var safeCall = new ExprCall(ctx, (Action<object, string, Delegate>)InternalFunctions.SafeAddEventListener, null, call.Obj, eventNameExpr, call.Args.First());
                            return safeCall;
                        }
                        var nameProp = jsDetail.Properties.FirstOrDefault(x => x.Name == "Name");
                        if (nameProp.Name != null) {
                            propertyName = (string)nameProp.Argument.Value;
                        }
                    }
                    if (propertyName == null) {
                        propertyName = JsCase(mDef.Name.Substring(4));
                    }
                    if (mDef.Name.Substring(4) == "Item") {
                        propertyName = null;
                    }
                    if (mDef.IsStatic) {
                        propertyName = JsCase(mDef.DeclaringType.Name) + "." + propertyName;
                    }
                    var jsProperty = new ExprJsResolvedProperty(ctx, call, propertyName);
                    return jsProperty;
                } else if (mDef.IsConstructor) {
                    string typeName = null;
                    if (jsDetail != null) {
                        var nameProp = jsDetail.Properties.FirstOrDefault(x => x.Name == "Name");
                        if (nameProp.Name != null) {
                            typeName = (string)nameProp.Argument.Value;
                        }
                    }
                    if (typeName == null) {
                        typeName = (string)jsClass.ConstructorArguments[0].Value;
                    }
                    var expr = new ExprJsResolvedCtor(ctx, typeName, mRef.DeclaringType, call.Args);
                    return expr;
                } else {
                    string methodName = JsName(mDef);
                    if (mDef.IsStatic) {
                        methodName = JsCase(mDef.DeclaringType.Name) + "." + methodName;
                    }
                    return new ExprJsResolvedMethod(ctx, call.Type, call.Obj, methodName, call.Args);
                }
            }
            if (jsClass != null) {
                return null;
            }
            var redirect = mDef.GetCustomAttribute<JsRedirectAttribute>(true);
            if (redirect != null) {
                var tRedirect = (TypeReference)redirect.ConstructorArguments[0].Value;
                if (tRedirect.HasGenericParameters) {
                    var args = ((GenericInstanceType)mRef.DeclaringType).GenericArguments.ToArray();
                    tRedirect = tRedirect.MakeGeneric(args);
                }
                var mRedirect = tRedirect.EnumResolvedMethods(mRef).First(x => x.MatchMethodOnly(mRef));
                var expr = new ExprCall(ctx, mRedirect, call.Obj, call.Args, call.IsVirtualCall, null, call.Type);
                return expr;
            }
            var mDeclTypeDef = mRef.DeclaringType.Resolve();
            var callType = mDeclTypeDef.LoadType();
            if (callType == null) {
                // Method is outside this module or its references
                return null;
            }
            var altType = map.ValueOrDefault(callType);
            if (altType != null) {
                var tRef = thisModule.Import(altType);
                if (tRef.HasGenericParameters) {
                    var args = ((GenericInstanceType)mRef.DeclaringType).GenericArguments.ToArray();
                    tRef = tRef.MakeGeneric(args);
                }
                var mappedMethod = tRef.EnumResolvedMethods().FirstOrDefault(x => {
                    var xResolved = x.FullResolve(mRef.DeclaringType, mRef, true);
                    var res = xResolved.Resolve();
                    var visible = res.IsPublic || (res.Name.Contains(".") && !res.IsConstructor); // to handle explicit interface methods
                    return visible && res.GetCustomAttribute<JsRedirectAttribute>(true) == null && xResolved.MatchMethodOnly(mRef);
                });
                if (mappedMethod != null) {
                    Expr expr;
                    if (call.ExprType == Expr.NodeType.NewObj) {
                        //if (mDef.IsConstructor) {
                        expr = new ExprNewObj(ctx, mappedMethod, call.Args);
                    } else {
                        mappedMethod = mappedMethod.FullResolve(mRef.DeclaringType, mRef);
                        expr = new ExprCall(ctx, mappedMethod, call.Obj, call.Args, call.IsVirtualCall);
                    }
                    return expr;
                }
                // Look for methods that generate AST
                var method = FindJsMember(call.CallMethod, altType);
                if (method != null && method.ReturnType() == typeof(Expr)) {
                    if (method.DeclaringType.ContainsGenericParameters) {
                        var tArgs = ((GenericInstanceType)call.CallMethod.DeclaringType).GenericArguments;
                        var typeArgs = tArgs.Select(x => x.LoadType()).ToArray();
                        var t = method.DeclaringType.MakeGenericType(typeArgs);
                        method = t.GetMethods().First(x => x.Name == method.Name && x.ReturnType == typeof(Expr));
                    }
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
            if (ctx.TDef.Methods.Any(x => x.IsExternal())) {
                // Type contains external methods, which cannot be loaded
                return null;
            }
            var typeFullName = ctx.TDef.AssemblyQualifiedName();
            var methodType = Type.GetType(typeFullName);
            if (methodType == null) {
                // Method is outside this module or its references
                return null;
            }
            var altType = map.ValueOrDefault(methodType);
            if (altType != null) {
                if (altType.IsGenericTypeDefinition) {
                    var args = ((GenericInstanceType)ctx.TRef).GenericArguments.Select(x => x.LoadType()).ToArray();
                    altType = altType.MakeGenericType(args);
                }
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
            if (tRef.IsNested) {
                return tRef;
            }
            GenericInstanceType tRefGen = null;
            if (tRef.IsGenericInstance) {
                tRefGen = (GenericInstanceType)tRef;
                tRef = tRef.Resolve();
            }
            var type = Type.GetType(tRef.FullName);
            if (type == null) {
                // Generic types cannot be got this way, but that's fine because they never need to be (so far...)
                return tRefGen ?? tRef;
            }
            var revType = reverseTypeMap.ValueOrDefault(type);
            if (revType == null) {
                return tRefGen ?? tRef;
            }
            var tRefRev = thisModule.Import(revType);
            if (tRefGen != null) {
                tRefRev = tRefRev.MakeGeneric(tRefGen.GenericArguments.ToArray());
            }
            return tRefRev;
        }

    }
}
