using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Utils {
    public static class CecilExtensions {

        //public static GenericInstanceType MakeGenericType(this TypeReference typeRef, params TypeReference[] args) {
        //    if (typeRef.GenericParameters.Count != args.Length) {
        //        throw new ArgumentException("Wrong number of generic arguments");
        //    }
        //    var ret = new GenericInstanceType(typeRef);
        //    foreach (var arg in args) {
        //        ret.GenericArguments.Add(arg);
        //    }
        //    return ret;
        //}

        //public static GenericInstanceMethod MakeGenericMethod(this MethodReference methodRef, params TypeReference[] args) {
        //    if (methodRef.GenericParameters.Count != args.Length) {
        //        throw new ArgumentException("Wrong number of generic arguments");
        //    }
        //    var ret = new GenericInstanceMethod(methodRef);
        //    foreach (var arg in args) {
        //        ret.GenericArguments.Add(arg);
        //    }
        //    return ret;
        //}

        public static TypeReference GetBaseType(this TypeReference tRef) {
            if (tRef == null) {
                throw new ArgumentNullException("typeRef");
            }
            if (tRef.IsDefinition) {
                return ((TypeDefinition)tRef).BaseType;
            }
            if (tRef.IsGenericInstance) {
                var tGenInst = (GenericInstanceType)tRef;
                var genericDef = tGenInst.GetElementType();
                var baseType = genericDef.GetBaseType();
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
            if (!TypeExtensions.TypeRefEqComparerInstance.Equals(a.ReturnType, b.ReturnType)) {
                return false;
            }
            var aParamTypes = a.Parameters.Select(x => x.ParameterType).ToArray();
            var bParamTypes = b.Parameters.Select(x => x.ParameterType).ToArray();
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
            case MetadataType.Object:
            case MetadataType.String:
                return self;
            case MetadataType.Class:
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
            var ordered = en.Select(x => new { item = x, type = selectType(x) }).OrderBy(x => x.type, TypeExtensions.BaseFirstComparerInstance).ToArray();
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

        public static IEnumerable<TypeReference> EnumThisAllBaseTypes(this TypeReference type) {
            var t = type;
            for (; ; ) {
                yield return t;
                t = t.GetBaseType();
                if (t == null) {
                    break;
                }
            }
        }

        public static IEnumerable<MethodReference> EnumResolvedMethods(this TypeReference type, IEnumerable<MethodReference> baseMethods) {
            var tDef = type.Resolve();
            foreach (var m in tDef.Methods) {
                var mScopes = baseMethods.Where(x => x.MatchMethodOnlyLoose(m)).DefaultIfEmpty().ToArray();
                foreach (var mScope in mScopes) {
                    var mResolved = m.FullResolve(type, mScope);
                    yield return mResolved;
                }
            }
        }

    }
}
