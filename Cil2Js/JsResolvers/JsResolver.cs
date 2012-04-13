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

        static JsResolver() {
            var thisModule = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location, AssemblyResolvers.ReaderParameters);
            Action<TypeDefinition, TypeDefinition> addWithNested = null;
            addWithNested = (bclType, customType) => {
                cMap.Add(bclType, customType);
                var bclNestedTypes = bclType.NestedTypes;
                if (bclNestedTypes.Any()) {
                    var customNestedTypes = customType.Resolve().NestedTypes;
                    foreach (var bclNestedType in bclNestedTypes) {
                        var customNestedType = customNestedTypes.FirstOrDefault(x => x.Name == bclNestedType.Name);
                        if (customNestedType != null) {
                            addWithNested(bclNestedType, customNestedType);
                        }
                    }
                }
            };
            cMap = new Dictionary<TypeDefinition, TypeDefinition>(TypeExtensions.TypeRefEqComparerInstance);
            foreach (var m in map) {
                var bclType = thisModule.Import(m.Key).Resolve();
                var customType = thisModule.Import(m.Value).Resolve();
                addWithNested(bclType, customType);
            }
        }

        private static Type T(string typeName) {
            return Type.GetType(typeName);
        }

        private static Dictionary<TypeDefinition, TypeDefinition> cMap;

        private static string JsCase(string s) {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        private static bool DoesMatchMethod(MethodReference mInternal, MethodReference m) {
            // Look for methods with custom signatures
            var detailsAttr = mInternal.Resolve().GetCustomAttribute<JsDetailAttribute>(true);
            if (detailsAttr != null) {
                var signature = detailsAttr.Properties.FirstOrDefault(x => x.Name == "Signature");
                if (signature.Name != null) {
                    if (mInternal.Name != m.Name) {
                        return false;
                    }
                    var sigTypes = ((CustomAttributeArgument[])signature.Argument.Value)
                        .Select(x => ((TypeReference)x.Value).FullResolve(m))
                        .ToArray();
                    var mReturnType = m.ReturnType.FullResolve(m);
                    if (!mReturnType.IsSame(sigTypes[0])) {
                        return false;
                    }
                    for (int i = 0; i < m.Parameters.Count; i++) {
                        var mParameterType = m.Parameters[i].ParameterType.FullResolve(m);
                        if (!mParameterType.IsSame(sigTypes[i + 1])) {
                            return false;
                        }
                    }
                    return true;
                }
            }
            // Look for C# method that matches with custom 'this'
            Func<bool> isFakeThis = () => {
                if (mInternal.HasThis) {
                    return false;
                }
                if (mInternal.Name != m.Name) {
                    return false;
                }
                if (mInternal.Parameters.Count != m.Parameters.Count + 1) {
                    return false;
                }
                if (mInternal.Parameters[0].GetCustomAttribute<JsFakeThisAttribute>() == null) {
                    return false;
                }
                if (!mInternal.ReturnType.IsSame(m.ReturnType)) {
                    return false;
                }
                for (int i = 1; i < mInternal.Parameters.Count; i++) {
                    if (!mInternal.Parameters[i].ParameterType.IsSame(m.Parameters[i - 1].ParameterType)) {
                        return false;
                    }
                }
                return true;
            };
            if (isFakeThis()) {
                return true;
            }
            // Look for C# method that match signature
            return mInternal.MatchMethodOnly(m);
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

        /// <summary>
        /// If a call/newobj requires translating to an Expr that is not a call/newobj, then it is done here.
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        public static Expr ResolveCallSite(ICall call) {
            var ctx = call.Ctx;
            var mRef = call.CallMethod;
            var tRefDecl = mRef.DeclaringType;
            var mDef = mRef.Resolve();
            // A call to a method in a "JsClass" class - all external methods/properties require translating to JS
            var tDefDecl = mDef.DeclaringType;
            var jsClassAttr = tDefDecl.GetCustomAttribute<JsClassAttribute>() ?? tDefDecl.GetCustomAttribute<JsAbstractClassAttribute>();
            if (jsClassAttr != null) {
                if (mDef.IsExternal()) {
                    var jsDetail = mDef.GetCustomAttribute<JsDetailAttribute>(true);
                    var jsDetailName = jsDetail.NullThru(x => (string)x.Properties.FirstOrDefault(y => y.Name == "Name").Argument.Value);
                    var jsDetailIsDomEventProp = jsDetail.NullThru(x => ((bool?)x.Properties.FirstOrDefault(y => y.Name == "IsDomEvent").Argument.Value) ?? false);
                    if (mDef.IsGetter || mDef.IsSetter) {
                        // Property access
                        if (jsDetailIsDomEventProp) {
                            // Special handling of DOM events
                            if (!mDef.IsSetter) {
                                throw new InvalidOperationException("Only setters supported on DOM events");
                            }
                            if (!mDef.Name.StartsWith("set_On")) {
                                throw new InvalidOperationException("DOM event name must start with 'On'");
                            }
                            if (call.Args.Count() != 1) {
                                throw new InvalidOperationException("DOM event setter must have exactly one argument");
                            }
                            var eventName = jsDetailName ?? mDef.Name.Substring(6).ToLowerInvariant();
                            var eventNameExpr = ctx.Literal(eventName);
                            var safeCallFunction = (Action<object, string, Delegate>)InternalFunctions.SafeAddEventListener;
                            var safeCall = new ExprCall(ctx, safeCallFunction, null, call.Obj, eventNameExpr, call.Args.First());
                            return safeCall;
                        } else {
                            var propertyName = jsDetailName ?? JsCase(mDef.Name.Substring(4));
                            if (mDef.Name.Substring(4) == "Item") {
                                propertyName = null;
                            } else if (mDef.IsStatic) {
                                propertyName = JsCase(mDef.DeclaringType.Name) + "." + propertyName;
                            }
                            var jsProperty = new ExprJsResolvedProperty(ctx, call, propertyName);
                            return jsProperty;
                        }
                    } else if (mDef.IsConstructor && !mDef.IsStatic) {
                        // Constructor new object call
                        var typeName = jsDetailName ?? (string)jsClassAttr.ConstructorArguments[0].Value;
                        var expr = new ExprJsResolvedCtor(ctx, typeName, tRefDecl, call.Args);
                        return expr;
                    } else {
                        // Normal method call
                        var methodName = jsDetailName ?? JsCase(mDef.Name);
                        if (mDef.IsStatic) {
                            methodName = JsCase(mDef.DeclaringType.Name) + "." + methodName;
                        }
                        var expr = new ExprJsResolvedMethod(ctx, call.Type, call.Obj, methodName, call.Args);
                        return expr;
                    }
                } else {
                    return null;
                }
            }
            var jsRedirectAttr = mDef.GetCustomAttribute<JsRedirectAttribute>(true);
            if (jsRedirectAttr != null) {
                if (jsRedirectAttr.ConstructorArguments[0].Value == null) {
                    return FindExprReturn(call, call.CallMethod.DeclaringType);
                }
                var redirectToTRef = ((TypeReference)jsRedirectAttr.ConstructorArguments[0].Value).FullResolve(mRef);
                var redirectToMRef = redirectToTRef.EnumResolvedMethods(mRef).First(x => x.MatchMethodOnly(mRef));
                switch (call.ExprType) {
                case Expr.NodeType.NewObj:
                    return new ExprNewObj(ctx, redirectToMRef, call.Args);
                case Expr.NodeType.Call:
                    return new ExprCall(ctx, redirectToMRef, call.Obj, call.Args, call.IsVirtualCall, null, call.Type);
                default:
                    throw new NotImplementedException("Cannot handle: " + call.ExprType);
                }
            }
            var exprRet = FindExprReturn(call);
            if (exprRet != null) {
                return exprRet;
            }
            return null;
        }

        /// <summary>
        /// Translate the ctx for transcoding here.
        /// Used to translate methods in the BCL to custom methods.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Ctx TranslateCtx(Ctx ctx) {
            var mappedMethod = FindMappedMethod(ctx.MRef);
            if (mappedMethod != null) {
                return new Ctx(mappedMethod.DeclaringType, mappedMethod, ctx);
            }
            return null;
        }

        private static MethodReference GetRedirect(MethodReference mRef) {
            var mDef = mRef.Resolve();
            var redirectAttr = mDef.GetCustomAttribute<JsRedirectAttribute>();
            if (redirectAttr == null) {
                return null;
            }
            var redirectType = (TypeReference)redirectAttr.ConstructorArguments[0].Value;
            if (redirectType == null) {
                return null;
            }
            if (redirectType.HasGenericParameters) {
                var genType = new GenericInstanceType(redirectType);
                foreach (var genArg in ((GenericInstanceType)mRef.DeclaringType).GenericArguments) {
                    genType.GenericArguments.Add(genArg);
                }
                redirectType = genType;
            }
            var redirectMethod = redirectType.EnumResolvedMethods(mRef).First(x => x.MatchMethodOnly(mRef));
            return redirectMethod;
        }

        private static Stmt FindStmtReturn(Ctx ctx) {
            var mRef = ctx.MRef;
            var mRefRedirected = GetRedirect(mRef);
            if (mRefRedirected != null) {
                mRef = mRefRedirected;
            }
            var mDef = mRef.Resolve();
            var jsAttr = mDef.GetCustomAttribute<JsAttribute>();
            if (jsAttr != null) {
                var ctorArgs = jsAttr.ConstructorArguments;
                if (ctorArgs.Count == 2) {
                    if (ctorArgs[0].Type.IsType() && ((Array)ctorArgs[1].Value).Length == 0) {
                        // IJsImpl class provides the implementation
                        var iJsImpltype = ((TypeReference)ctorArgs[0].Value).LoadType();
                        var iJsImpl = (IJsImpl)Activator.CreateInstance(iJsImpltype);
                        var stmt = iJsImpl.GetImpl(ctx);
                        return stmt;
                    }
                } else if (ctorArgs.Count == 1) {
                    if (ctorArgs[0].Type.IsString()) {
                        // Explicit JS string provides implementation
                        var js = (string)ctorArgs[0].Value;
                        var parameters = mRef.Parameters.Select((x, i) => ctx.MethodParameter(i, ((char)('a' + i)).ToString())).ToArray();
                        var stmt = new StmtJsExplicit(ctx, js, parameters.Concat(ctx.ThisNamed));
                        return stmt;
                    }
                }
            }
            var mappedType = TypeMap(ctx.TRef);
            if (mappedType == null && mDef.GetCustomAttribute<JsRedirectAttribute>() != null) {
                mappedType = ctx.TRef;
            }
            if (mappedType != null) {
                var mappedMRef = FindJsMember(mRef, mappedType);
                if (mappedMRef != null) {
                    if (mappedMRef.ReturnType.FullName == typeof(Stmt).FullName) {
                        var m = mappedMRef.LoadMethod();
                        var stmt = (Stmt)m.Invoke(null, new object[] { ctx });
                        return stmt;
                    }
                }
            }
            return null;
        }

        private static Expr FindExprReturn(ICall call, TypeReference forceMappedType = null) {
            var mRef = call.CallMethod;
            var mappedType = forceMappedType ?? TypeMap(mRef.DeclaringType);
            if (mappedType != null) {
                var mappedMRef = FindJsMember(mRef, mappedType);
                if (mappedMRef != null) {
                    if (mappedMRef.ReturnType.FullName == typeof(Expr).FullName) {
                        var m = mappedMRef.LoadMethod();
                        var expr = (Expr)m.Invoke(null, new object[] { call });
                        return expr;
                    }
                }
            }
            return null;
        }

        private static MethodReference FindMappedMethod(MethodReference mRef) {
            var mappedType = TypeMap(mRef.DeclaringType);
            if (mappedType != null) {
                var methods = mappedType.EnumResolvedMethods(mRef);
                var method = methods.FirstOrDefault(x => DoesMatchMethod(x, mRef));
                if (method == null) {
                    throw new NotImplementedException("Mapped method not implemented: " + mRef);
                }
                return method;
            }
            return null;
        }

        private static MethodReference FindJsMember(MethodReference mRef, TypeReference mappedTRef) {
            Func<IEnumerable<CustomAttribute>, string, bool> isMatch = (attrs, mName) => {
                return attrs.Any(a => {
                    string aMethodName = null;
                    TypeReference aReturnType = null;
                    IEnumerable<TypeReference> aParameterTypes = null;
                    bool? isStatic = null;
                    var numArgs = a.ConstructorArguments.Count;
                    var ctorArgs = a.ConstructorArguments;
                    if (numArgs == 0 || (numArgs == 1 && ctorArgs[0].Type.IsString())) {
                        // Do nothing
                    } else if (numArgs == 2) {
                        aReturnType = (TypeReference)ctorArgs[0].Value;
                        aParameterTypes = ((IEnumerable<CustomAttributeArgument>)ctorArgs[1].Value).Select(x => (TypeReference)x.Value).ToArray();
                    } else if (numArgs == 3) {
                        aMethodName = (string)ctorArgs[0].Value;
                        aReturnType = (TypeReference)ctorArgs[1].Value;
                        aParameterTypes = ((IEnumerable<CustomAttributeArgument>)ctorArgs[2].Value).Select(x => (TypeReference)x.Value).ToArray();
                    } else {
                        throw new InvalidOperationException("Unrecognised JsAttribute constructor");
                    }
                    if (isStatic != null) {
                        if (isStatic.Value == mRef.HasThis) {
                            return false;
                        }
                    }
                    if (aMethodName == null && aParameterTypes == null && mName == mRef.Name) {
                        return true;
                    }
                    if ((aMethodName ?? mName) != mRef.Name) {
                        return false;
                    }
                    if (aParameterTypes.Count() != mRef.Parameters.Count) {
                        return false;
                    }
                    if (aParameterTypes.Zip(mRef.Parameters, (x, y) => new { aType = x, mRefType = y.ParameterType }).Any(x => {
                        var aTypeResolved = x.aType.FullResolve(mRef);
                        var mRefTypeResolved = x.mRefType.FullResolve(mRef);
                        return !aTypeResolved.IsSame(mRefTypeResolved);
                    })) {
                        return false;
                    }
                    var aReturnTypeResolved = aReturnType.FullResolve(mRef);
                    var mRefReturnTypeResolved = mRef.ReturnType.FullResolve(mRef);
                    if (!aReturnTypeResolved.IsSame(mRefReturnTypeResolved)) {
                        return false;
                    }
                    return true;
                });
            };
            var members = mappedTRef.EnumResolvedMethods(mRef);
            var member = members.FirstOrDefault(m => {
                var mr = m.Resolve();
                var attrs = mr.GetCustomAttributes<JsAttribute>();
                var match = isMatch(attrs, m.Name);
                return match;
            });
            return member;
        }

        /// <summary>
        /// If a method to directly provide an AST for a method is available, then find and use it here.
        /// The incoming ctx will contain the BCL method, not the custom method.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Stmt ResolveMethod(Ctx ctx) {
            var stmt = FindStmtReturn(ctx);
            if (stmt != null) {
                return stmt;
            }
            return null;
        }

        private static TypeReference PerformMapping(Dictionary<TypeDefinition, TypeDefinition> map, TypeReference tRef) {
            if (tRef.IsGenericParameter) {
                return null;
            }
            var tDef = tRef.Resolve();
            var altTDef = map.ValueOrDefault(tDef);
            if (altTDef == null) {
                return null;
            }
            if (altTDef.HasGenericParameters) {
                var tRefGenInst = (GenericInstanceType)tRef;
                var ret = altTDef.MakeGeneric(tRefGenInst.GenericArguments.ToArray());
                return ret;
            }
            return altTDef;
        }

        public static TypeReference TypeMap(TypeReference tRef) {
            return PerformMapping(cMap, tRef);
        }

    }
}
