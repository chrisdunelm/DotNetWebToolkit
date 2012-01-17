using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Utils {
    public static class CecilExtensions {

        public static TypeReference GetBaseType(this TypeReference tRef) {
            if (tRef == null) {
                throw new ArgumentNullException("typeRef");
            }
            if (tRef.IsArray) {
                var tArray = tRef.Module.Import(typeof(Array));
                return tArray;
            }
            if (tRef.IsDefinition) {
                return ((TypeDefinition)tRef).BaseType;
            }
            if (tRef.IsGenericInstance) {
                var tGenInst = (GenericInstanceType)tRef;
                var genericDef = tGenInst.GetElementType();
                var baseType = genericDef.GetBaseType();
                if (baseType == null) {
                    // Generic interfaces may have no base type
                    return null;
                }
                if (baseType.IsGenericInstance) {
                    var baseTypeGenInst = (GenericInstanceType)baseType;
                    var baseTypeWithGenArgs = new GenericInstanceType(baseType.GetElementType());
                    foreach (var genArg in baseTypeGenInst.GenericArguments) {
                        if (genArg.IsGenericParameter) {
                            var genArgGenParam = (GenericParameter)genArg;
                            var arg = tGenInst.GenericArguments[genArgGenParam.Position];
                            baseTypeWithGenArgs.GenericArguments.Add(arg);
                        } else {
                            baseTypeWithGenArgs.GenericArguments.Add(genArg);
                        }
                    }
                    return baseTypeWithGenArgs;
                } else {
                    return baseType;
                }
            }
            return tRef.Resolve().BaseType;
        }

        public static MethodReference GetBaseMethod(this MethodReference mRef, MethodReference scopeMethod) {
            var mDef = mRef.Resolve();
            if (mDef.IsNewSlot) {
                return null;
            }
            if (!mDef.IsVirtual) {
                return null;
            }
            var baseType = mRef.DeclaringType.GetBaseType().FullResolve(mRef);
            if (baseType == null) {
                return null;
            }
            for (; ; ) {
                var methods = baseType.EnumResolvedMethods(new[] { scopeMethod }).ToArray();
                foreach (var m in methods) {
                    if (mRef.MatchMethodOnly(m)) {
                        return m;
                    }
                }
                baseType = baseType.GetBaseType();
                if (baseType == null) {
                    break;
                }
            }
            throw new InvalidOperationException("Cannot find base method");
        }

        public static MethodReference GetBasemostMethod(this MethodReference mRef, MethodReference scopeMethod) {
            var m = mRef;
            for (; ; ) {
                var curM = m;
                m = m.GetBaseMethod(scopeMethod);
                if (m == null) {
                    return curM;
                }
            }
        }

        public static bool MatchMethodOnly(this MethodReference a, MethodReference b) {
            if (a.Name != b.Name) {
                return false;
            }
            if (a.IsGenericInstance != b.IsGenericInstance) {
                return false;
            }
            if (a.GenericParameters.Count != b.GenericParameters.Count) {
                return false;
            }
            var aReturnType = a.ReturnType.FullResolve(a);
            var bReturnType = b.ReturnType.FullResolve(b);
            if (!TypeExtensions.TypeRefEqComparerInstance.Equals(aReturnType, bReturnType)) {
                return false;
            }
            var aParamTypes = a.Parameters.Select(x => x.ParameterType.FullResolve(a)).ToArray();
            var bParamTypes = b.Parameters.Select(x => x.ParameterType.FullResolve(b)).ToArray();
            if (!aParamTypes.SequenceEqual(bParamTypes, TypeExtensions.TypeRefEqComparerInstance)) {
                return false;
            }
            if (a.IsGenericInstance) {
                var aGenInst = (GenericInstanceMethod)a;
                var bGenInst = (GenericInstanceMethod)b;
                if (!aGenInst.GenericArguments.SequenceEqual(bGenInst.GenericArguments, TypeExtensions.TypeRefEqComparerInstance)) {
                    return false;
                }
            }
            return true;
        }

        public static bool MatchMethodOnlyLoose(this MethodReference a, MethodReference b) {
            if (a.Name != b.Name) {
                return false;
            }
            if (a.Parameters.Count != b.Parameters.Count) {
                return false;
            }
            var aArgCount = a.IsGenericInstance ? ((GenericInstanceMethod)a).GenericArguments.Count : a.GenericParameters.Count;
            var bArgCount = b.IsGenericInstance ? ((GenericInstanceMethod)b).GenericArguments.Count : b.GenericParameters.Count;
            if (aArgCount != bArgCount) {
                return false;
            }
            return true;
        }

        public static TypeReference FullResolve(this TypeReference self, Ctx ctx) {
            return self.FullResolve(ctx.TRef, ctx.MRef);
        }

        public static TypeReference FullResolve(this TypeReference self, MethodReference scope) {
            return self.FullResolve(scope.DeclaringType, scope);
        }

        public static TypeReference FullResolve(this TypeReference self, FieldReference scope) {
            return self.FullResolve(scope.DeclaringType, null);
        }

        public static TypeReference FullResolve(this TypeReference self, TypeReference scopeType, MethodReference scopeMethod) {
            switch (self.MetadataType) {
            case MetadataType.Void:
            case MetadataType.Boolean:
            case MetadataType.Byte:
            case MetadataType.SByte:
            case MetadataType.Int16:
            case MetadataType.Int32:
            case MetadataType.Int64:
            case MetadataType.UInt16:
            case MetadataType.UInt32:
            case MetadataType.UInt64:
            case MetadataType.Single:
            case MetadataType.Double:
            case MetadataType.IntPtr:
            case MetadataType.UIntPtr:
            case MetadataType.Char:
            case MetadataType.Object:
            case MetadataType.String:
                return self;
            case MetadataType.Class:
            case MetadataType.ValueType:
                if (self.HasGenericParameters) {
                    var ret = new GenericInstanceType(self);
                    foreach (var arg in self.GenericParameters) {
                        var argResolved = arg.FullResolve(scopeType, scopeMethod);
                        ret.GenericArguments.Add(argResolved);
                    }
                    return ret;
                }
                return self;
            case MetadataType.GenericInstance:
                var genInst = (GenericInstanceType)self;
                var genArgs = genInst.GenericArguments.Select(x => x.FullResolve(scopeType, scopeMethod)).ToArray();
                if (genInst.GenericArguments.SequenceEqual(genArgs)) {
                    return self;
                } else {
                    var ret = new GenericInstanceType(genInst.ElementType);
                    foreach (var arg in genArgs) {
                        ret.GenericArguments.Add(arg);
                    }
                    return ret;
                }
            case MetadataType.Var: {
                    var scope = (GenericInstanceType)scopeType;
                    var param = (GenericParameter)self;
                    var type = scope.GenericArguments[param.Position];
                    return type;
                }
            case MetadataType.MVar: {
                    if (scopeMethod == null) {
                        return self;
                    }
                    var scope = (GenericInstanceMethod)scopeMethod;
                    var param = (GenericParameter)self;
                    var type = scope.GenericArguments[param.Position];
                    return type;
                }
            case MetadataType.Array: {
                    var array = (ArrayType)self;
                    var elType = array.ElementType.FullResolve(scopeType, scopeMethod);
                    if (elType != array.ElementType) {
                        return new ArrayType(elType);
                    } else {
                        return self;
                    }
                }
            case MetadataType.Pointer: {
                    var ptr = (PointerType)self;
                    var elType = ptr.ElementType.FullResolve(scopeType, scopeMethod);
                    if (elType != ptr.ElementType) {
                        return new PointerType(elType);
                    } else {
                        return self;
                    }
                }
            default:
                throw new NotImplementedException("Cannot handle: " + self.MetadataType);
            }
        }

        public static MethodReference FullResolve(this MethodReference self, Ctx ctx) {
            return self.FullResolve(ctx.TRef, ctx.MRef);
        }

        public static MethodReference FullResolve(this MethodReference self, TypeReference scopeType, MethodReference scopeMethod) {
            MethodReference m = null;
            var declType = self.DeclaringType.FullResolve(scopeType, scopeMethod);
            if (declType != self.DeclaringType) {
                m = new MethodReference(self.Name, self.ReturnType, declType) {
                    ExplicitThis = self.ExplicitThis,
                    HasThis = self.HasThis,
                    CallingConvention = self.CallingConvention,
                    MetadataToken = self.MetadataToken,
                };
                foreach (var p in self.Parameters) {
                    m.Parameters.Add(p);
                }
                if (self.IsGenericInstance) {
                    var selfGenInst = (GenericInstanceMethod)self;
                    if (selfGenInst.GenericArguments.Any(x => x.IsGenericParameter)) {
                        if (!selfGenInst.GenericArguments.All(x => x.IsGenericParameter)) {
                            throw new InvalidOperationException("Either all or none should be generic parameters");
                        }
                        foreach (var a in selfGenInst.GenericArguments) {
                            var gp = new GenericParameter(a.Name, m);
                            m.GenericParameters.Add(gp);
                        }
                    }
                } else {
                    foreach (var a in self.GenericParameters) {
                        m.GenericParameters.Add(a);
                    }
                }
            }
            if (self.IsGenericInstance) {
                var selfGenInst = (GenericInstanceMethod)self;
                var genArgs = selfGenInst.GenericArguments.Select(x => x.FullResolve(scopeType, scopeMethod)).ToArray();
                if (!genArgs.SequenceEqual(selfGenInst.GenericArguments)) {
                    var m2 = new GenericInstanceMethod(m ?? selfGenInst.ElementMethod);
                    foreach (var genArg in genArgs) {
                        m2.GenericArguments.Add(genArg);
                    }
                    return m2;
                }
            }
            var mSelf = m ?? self;
            if (mSelf.HasGenericParameters) {
                var m2 = new GenericInstanceMethod(mSelf);
                foreach (var genParam in mSelf.GenericParameters) {
                    var genArg = genParam.FullResolve(scopeType, scopeMethod);
                    m2.GenericArguments.Add(genArg);
                }
                return m2;
            }
            return mSelf;
        }

        public static FieldReference FullResolve(this FieldReference self, Ctx ctx) {
            return self.FullResolve(ctx.TRef, ctx.MRef);
        }

        public static FieldReference FullResolve(this FieldReference self, TypeReference scopeType, MethodReference scopeMethod) {
            var declType = self.DeclaringType.FullResolve(scopeType, scopeMethod);
            if (declType != self.DeclaringType) {
                var f = new FieldReference(self.Name, self.FieldType, declType) {
                    MetadataToken = self.MetadataToken,
                };
                return f;
            }
            return self;
        }

        public static void TypeTreeTraverse<T, TState>(this IEnumerable<T> en, Func<T, TypeReference> selectType, Func<T, TState, TState> action, TState initState = default(TState)) {
            var ordered = en.Select(x => new { item = x, type = selectType(x) }).OrderByBaseFirst(x => x.type).ToArray();
            var states = new Dictionary<TypeReference, TState>(TypeExtensions.TypeRefEqComparerInstance);
            foreach (var x in ordered) {
                var baseType = x.type.GetBaseType();
                var state = initState;
                while (baseType != null) {
                    TState getState;
                    if (states.TryGetValue(baseType, out getState)) {
                        state = getState;
                        break;
                    }
                    baseType = baseType.GetBaseType();
                }
                var newState = action(x.item, state);
                states.Add(x.type, newState);
            }
        }

        public static bool IsGenericInstanceOf(this MethodReference mGenInst, MethodReference m) {
            if (!mGenInst.IsGenericInstance) {
                return false;
            }
            if (m.IsGenericInstance) {
                var genInst = (GenericInstanceMethod)mGenInst;
                var m2 = (GenericInstanceMethod)m;
                return TypeExtensions.MethodRefEqComparerInstance.Equals(genInst.ElementMethod, m2.ElementMethod);
            } else {
                if (!m.HasGenericParameters) {
                    return false;
                }
                var genInst = (GenericInstanceMethod)mGenInst;
                return TypeExtensions.MethodRefEqComparerInstance.Equals(genInst.ElementMethod, m);
            }
        }

        public static bool IsImplementationOf(this MethodReference method, MethodReference iFaceMethod) {
            // TODO: This way of sorting out interfaces is not efficient.
            // Better to have a method that returns the interface map for a type
            var mDef = method.Resolve();
            if (mDef.Overrides.Any(x => {
                var xResolved = x.FullResolve(method.DeclaringType, method);
                return TypeExtensions.MethodRefEqComparerInstance.Equals(xResolved, iFaceMethod);
            })) {
                return true;
            }
            var allMethods = method.DeclaringType.FullResolve(method).EnumResolvedMethods(method, iFaceMethod).ToArray();
            if (allMethods.SelectMany(x => x.Resolve().Overrides).Any(x => {
                var xResolved = x.FullResolve(method.DeclaringType, method);
                return TypeExtensions.MethodRefEqComparerInstance.Equals(xResolved, iFaceMethod);
            })) {
                return false;
            }
            return method.MatchMethodOnly(iFaceMethod);
        }

        public static IEnumerable<TypeReference> EnumThisAllBaseTypes(this TypeReference type) {
            var t = type;
            do {
                yield return t;
                t = t.GetBaseType();
            } while (t != null);
        }

        public static IEnumerable<MethodReference> EnumResolvedMethods(this TypeReference type, params MethodReference[] baseMethods) {
            return type.EnumResolvedMethods((IEnumerable<MethodReference>)baseMethods);
        }

        public static IEnumerable<MethodReference> EnumResolvedMethods(this TypeReference type, IEnumerable<MethodReference> baseMethods) {
            if (type.IsArray) {
                // Special processing for arrays - they are not normal - add all generic interface methods
                var module = type.Module;
                var elType = type.GetElementType();
                var arrayMethodsType = module.Import(typeof(GenericArrayMethods<>)).MakeGeneric(elType);
                var ms = arrayMethodsType.EnumResolvedMethods().ToArray();
                return ms;
            } else {
                var ret = new List<MethodReference>();
                var tDef = type.Resolve();
                foreach (var m in tDef.Methods) {
                    var mScopes = baseMethods.EmptyIfNull().Where(x => x.MatchMethodOnlyLoose(m)).DefaultIfEmpty().ToArray();
                    foreach (var mScope in mScopes) {
                        var mResolved = m.FullResolve(type, mScope);
                        ret.Add(mResolved);
                    }
                }
                return ret;
            }
        }

        [DebuggerStepThrough]
        public static bool IsExternal(this MethodDefinition method) {
            return method.HasBody && method.RVA == 0;
        }

        public static TypeReference MakeGeneric(this TypeReference t, params TypeReference[] args) {
            if (t.GenericParameters.Count != args.Length) {
                throw new ArgumentException("Wrong number of generic arguments");
            }
            var tGeneric = new GenericInstanceType(t);
            foreach (var arg in args) {
                tGeneric.GenericArguments.Add(arg);
            }
            return tGeneric;
        }

        public static TypeReference MakeArray(this TypeReference t) {
            return new ArrayType(t);
        }

        public static MethodReference MakeGeneric(this MethodReference m, params TypeReference[] args) {
            if (m.GenericParameters.Count != args.Length) {
                throw new ArgumentException("Wrong number of generic arguments");
            }
            var mGeneric = new GenericInstanceMethod(m);
            foreach (var arg in args) {
                mGeneric.GenericArguments.Add(arg);
            }
            return mGeneric;
        }

        /// <summary>
        /// Returns all interfaces that 'type' implements, including inheritance
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<TypeReference> EnumAllInterfaces(this TypeReference type) {
            if (type.IsArray) {
                // Special processing for arrays - they are not normal
                var elType = type.GetElementType();
                var module = type.Module;
                var nonGenericIFaces = module.Import(typeof(Array)).EnumAllInterfaces();
                var iListGeneric = module.Import(typeof(IList<>)).MakeGeneric(elType);
                var iCollectionGeneric = module.Import(typeof(ICollection<>)).MakeGeneric(elType);
                var iEnumerableGeneric = module.Import(typeof(IEnumerable<>)).MakeGeneric(elType);
                var arrayIFaces = nonGenericIFaces.Concat(new[] { iListGeneric, iCollectionGeneric, iEnumerableGeneric }).ToArray();
                return arrayIFaces;
            }
            var allIFaces = type.EnumThisAllBaseTypes().SelectMany(x => {
                var xDef = x.Resolve();
                var iFaces = xDef.Interfaces;
                var resolvedIFaces = iFaces.Select(y => y.FullResolve(x, null)).ToArray();
                return resolvedIFaces;
            }).ToArray();
            return allIFaces;
        }

        public static bool DoesImplement(this TypeReference t, TypeReference iFace) {
            return t.EnumAllInterfaces().Any(x => x.IsSame(iFace));
        }

        public static CustomAttribute GetCustomAttribute<TAttr>(this TypeDefinition t) {
            return t.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == typeof(TAttr).Name);
        }

        public static CustomAttribute GetCustomAttribute<TAttr>(this MethodDefinition m) {
            return m.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == typeof(TAttr).Name);
        }

    }
}
