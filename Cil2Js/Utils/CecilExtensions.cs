using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
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
            var aDef = a.Resolve();
            var bDef = b.Resolve();
            if (aDef == null || bDef == null) {
                return false;
            }
            if (aDef.IsStatic != bDef.IsStatic) {
                return false;
            }
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
                var aGenArgs = aGenInst.GenericArguments;
                var bGenArgs = bGenInst.GenericArguments;
                if (!aGenArgs.SequenceEqual(bGenArgs, TypeExtensions.TypeRefEqComparerInstance)) {
                    return false;
                }
            }
            return true;
        }

        public static bool MatchMethodOnlyLoose(this MethodReference a, MethodReference b) {
            // HACK: This whole method needs to go. All users of this method are able to use
            // .MatchMethodOnly() with a bit of work
            if (a.Name != b.Name && !(b.Name.Length > a.Name.Length && b.Name.EndsWith(a.Name) && b.Name[b.Name.Length - a.Name.Length - 1] == '.')) {
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

        public static TypeReference FullResolve(this TypeReference self, TypeReference scopeType, MethodReference scopeMethod, bool allowFailure = false) {
            var selfDef = self.Resolve();
            if (selfDef != null) {
                var jsUseTypeAttr = selfDef.GetCustomAttribute<JsUseTypeAttribute>();
                if (jsUseTypeAttr != null) {
                    var useType = (TypeReference)jsUseTypeAttr.ConstructorArguments[0].Value;
                    self = self.Module.Import(useType);
                }
            }
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
                        var argResolved = arg.FullResolve(scopeType, scopeMethod, allowFailure);
                        ret.GenericArguments.Add(argResolved);
                    }
                    return ret;
                }
                if (self.FullName == typeof(GenTypeParam0).FullName) {
                    return ((GenericInstanceType)scopeType).GenericArguments[0];
                }
                if (self.FullName == typeof(GenTypeParam1).FullName) {
                    return ((GenericInstanceType)scopeType).GenericArguments[1];
                }
                if (self.FullName == typeof(GenTypeParam2).FullName) {
                    return ((GenericInstanceType)scopeType).GenericArguments[2];
                }
                if (self.FullName == typeof(GenTypeParam3).FullName) {
                    return ((GenericInstanceType)scopeType).GenericArguments[3];
                }
                if (self.FullName == typeof(GenMethodParam0).FullName) {
                    return ((GenericInstanceMethod)scopeMethod).GenericArguments[0];
                }
                if (self.FullName == typeof(GenMethodParam1).FullName) {
                    return ((GenericInstanceMethod)scopeMethod).GenericArguments[1];
                }
                if (self.FullName == typeof(GenMethodParam2).FullName) {
                    return ((GenericInstanceMethod)scopeMethod).GenericArguments[2];
                }
                if (self.FullName == typeof(GenMethodParam3).FullName) {
                    return ((GenericInstanceMethod)scopeMethod).GenericArguments[3];
                }
                return self;
            case MetadataType.GenericInstance:
                var genInst = (GenericInstanceType)self;
                var genArgs = genInst.GenericArguments.Select(x => x.FullResolve(scopeType, scopeMethod, allowFailure)).ToArray();
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
                    var scope = scopeType.ElementType() as GenericInstanceType;
                    var param = self as GenericParameter;
                    if (scope == null || param == null) {
                        if (allowFailure) {
                            return self;
                        }
                        throw new InvalidCastException();
                    }
                    if (param.Position >= scope.GenericArguments.Count && allowFailure) {
                        return self;
                    }
                    var type = scope.GenericArguments[param.Position];
                    return type;
                }
            case MetadataType.MVar: {
                    var scope = scopeMethod as GenericInstanceMethod;
                    var param = self as GenericParameter;
                    if (scope == null || param == null) {
                        if (allowFailure) {
                            return self;
                        }
                        throw new InvalidCastException();
                    }
                    if (param.Position >= scope.GenericArguments.Count && allowFailure) {
                        return self;
                    }
                    var type = scope.GenericArguments[param.Position];
                    return type;
                }
            case MetadataType.Array: {
                    var array = (ArrayType)self;
                    var elType = array.ElementType.FullResolve(scopeType, scopeMethod, allowFailure);
                    if (elType != array.ElementType) {
                        return new ArrayType(elType);
                    } else {
                        return self;
                    }
                }
            case MetadataType.Pointer: {
                    var ptr = (PointerType)self;
                    var elType = ptr.ElementType.FullResolve(scopeType, scopeMethod, allowFailure);
                    if (elType != ptr.ElementType) {
                        return new PointerType(elType);
                    } else {
                        return self;
                    }
                }
            case MetadataType.ByReference: {
                    var byRef = (ByReferenceType)self;
                    var elType = byRef.ElementType.FullResolve(scopeType, scopeMethod, allowFailure);
                    if (elType != byRef.ElementType) {
                        return new ByReferenceType(elType);
                    } else {
                        return self;
                    }
                }
            case MetadataType.RequiredModifier: {
                    var reqMod = (RequiredModifierType)self;
                    var elType = reqMod.ElementType.FullResolve(scopeType, scopeMethod);
                    return elType;
                }
            default:
                throw new NotImplementedException("Cannot handle: " + self.MetadataType);
            }
        }

        public static MethodReference FullResolve(this MethodReference self, Ctx ctx) {
            return self.FullResolve(ctx.TRef, ctx.MRef);
        }

        public static MethodReference FullResolve(this MethodReference self, TypeReference scopeType, MethodReference scopeMethod, bool allowFailure = false) {
            // Resolve declaring type. Create new MethodReference if different
            // Convert method with GenericParameters to new GenericInstanceMethod
            // Convert GenericInstanceMethod with generic parameters to new GenericInstanceMethod with resolved parameters
            // Return type and parameters types must not be resolved - Cecil requires them to be generic-parameters
            var declType = self.DeclaringType.FullResolve(scopeType, scopeMethod, allowFailure);
            MethodReference mDeclType = null;
            MethodReference ret;
            if (!declType.IsSame(self.DeclaringType)) {
                mDeclType = new MethodReference(self.Name, self.ReturnType, declType) {
                    ExplicitThis = self.ExplicitThis,
                    HasThis = self.HasThis,
                    CallingConvention = self.CallingConvention,
                    MetadataToken = self.MetadataToken,
                };
                foreach (var p in self.Parameters) {
                    mDeclType.Parameters.Add(p);
                }
                if (self.IsGenericInstance) {
                    var selfGenInst = (GenericInstanceMethod)self;
                    foreach (var p in selfGenInst.ElementMethod.GenericParameters) {
                        mDeclType.GenericParameters.Add(p);
                    }
                    var mDeclTypeGenInst = new GenericInstanceMethod(mDeclType);
                    foreach (var p in selfGenInst.GenericArguments) {
                        mDeclTypeGenInst.GenericArguments.Add(p);
                    }
                    mDeclType = mDeclTypeGenInst;
                } else {
                    foreach (var p in self.GenericParameters) {
                        mDeclType.GenericParameters.Add(p);
                    }
                }
            }
            if (self.HasGenericParameters) {
                var mGenInst = new GenericInstanceMethod(mDeclType ?? self);
                foreach (var p in self.GenericParameters) {
                    var pResolved = p.FullResolve(scopeType, scopeMethod, allowFailure);
                    mGenInst.GenericArguments.Add(pResolved);
                }
                ret = mGenInst;
            } else if (self.IsGenericInstance) {
                var mGenInst = (GenericInstanceMethod)(mDeclType ?? self);
                if (mGenInst.GenericArguments.Any(x => x.IsGenericParameter)) {
                    var mGenInstRet = new GenericInstanceMethod(mGenInst.ElementMethod);
                    foreach (var p in mGenInst.GenericArguments) {
                        var pResolved = p.FullResolve(scopeType, scopeMethod, allowFailure);
                        mGenInstRet.GenericArguments.Add(pResolved);
                    }
                    ret = mGenInstRet;
                } else {
                    ret = mGenInst;
                }
            } else {
                ret = mDeclType ?? self;
            }
            // For verification that the resolved method is OK - can be removed later
            if (!allowFailure) {
                if (ret.ContainsGenericParameters()) {
                    throw new Exception("Return should not have generic parameters");
                }
            }
            var mDef = ret.Resolve();
            if (mDef == null) {
                throw new Exception("FullResolve() created unresolvable method");
            }
            return ret;
        }

        public static FieldReference FullResolve(this FieldReference self, Ctx ctx) {
            return self.FullResolve(ctx.TRef, ctx.MRef);
        }

        public static FieldReference FullResolve(this FieldReference self, TypeReference scopeType, MethodReference scopeMethod) {
            var declType = self.DeclaringType.FullResolve(scopeType, scopeMethod);
            FieldReference ret;
            if (declType != self.DeclaringType) {
                var f = new FieldReference(self.Name, self.FieldType, declType) {
                    MetadataToken = self.MetadataToken,
                };
                ret = f;
            } else {
                ret = self;
            }
            // Verify - can be removed later
            if (ret.Resolve() == null) {
                throw new Exception("Cannot resolve field");
            }
            return ret;
        }

        public static void TypeTreeTraverse<T, TState>(this IEnumerable<T> en, Func<T, TypeReference> selectType, Func<T, TState, TState> action, TState initState = default(TState)) {
            var ordered = en.Select(x => new { item = x, type = selectType(x) }).OrderByReferencedFirst(x => x.type).ToArray();
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
            // Maybe better to have a method that returns the interface map for a type - or cache results?
            var mDef = method.Resolve();
            if (mDef == null) {
                return false;
            }
            if (mDef.Overrides.Any(x => {
                var xResolved = x.FullResolve(method.DeclaringType, method, true);
                return TypeExtensions.MethodRefEqComparerInstance.Equals(xResolved, iFaceMethod);
            })) {
                return true;
            }
            var allMethods = method.DeclaringType.FullResolve(method).EnumResolvedMethods(method, iFaceMethod).ToArray();
            if (allMethods.SelectMany(x => {
                var xResolved = x.Resolve();
                if (xResolved != null) {
                    return x.Resolve().Overrides;
                } else {
                    return Enumerable.Empty<MethodReference>();
                }
            }).Any(x => {
                var xResolved = x.FullResolve(method.DeclaringType, method, true);
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

        public static IEnumerable<TypeReference> EnumThisAllContainedTypes(this TypeReference type) {
            yield return type;
            if (type.IsArray) {
                var elementType = ((ArrayType)type).ElementType;
                foreach (var t in elementType.EnumThisAllContainedTypes()) {
                    yield return t;
                }
            }
            if (type.IsGenericInstance) {
                var gen = (GenericInstanceType)type;
                foreach (var genParam in gen.GenericArguments) {
                    foreach (var t in genParam.EnumThisAllContainedTypes()) {
                        yield return t;
                    }
                }
            }
        }

        public static IEnumerable<MethodReference> EnumResolvedMethods(this TypeReference type, params MethodReference[] baseMethods) {
            return type.EnumResolvedMethods((IEnumerable<MethodReference>)baseMethods);
        }

        public static IEnumerable<MethodReference> EnumResolvedMethods(this TypeReference type, IEnumerable<MethodReference> baseMethods) {
            if (type.IsArray) {
                // Special processing for arrays - they are not normal - add all generic interface methods
                var module = type.Module;
                var elType = ((ArrayType)type).ElementType;
                var arrayMethodsType = module.Import(typeof(GenericArrayMethods<>)).MakeGeneric(elType);
                var ms = arrayMethodsType.EnumResolvedMethods().ToArray();
                return ms;
            } else {
                var ret = new List<MethodReference>();
                var tDef = type.Resolve();
                foreach (var m in tDef.Methods) {
                    var mScopes = baseMethods.EmptyIfNull().Where(x => x.MatchMethodOnlyLoose(m)).DefaultIfEmpty().ToArray();
                    foreach (var mScope in mScopes) {
                        var mResolved = m.FullResolve(type, mScope, true);
                        ret.Add(mResolved);
                    }
                }
                return ret;
            }
        }

        public static IEnumerable<FieldReference> EnumResolvedFields(this TypeReference type) {
            var tDef = type.Resolve();
            foreach (var field in tDef.Fields) {
                var fResolved = field.FullResolve(type, null);
                yield return fResolved;
            }
        }

        public static FieldReference GetField(this TypeReference type, string fieldName) {
            return type.EnumResolvedFields().First(x => x.Name == fieldName);
        }

        [DebuggerStepThrough]
        public static bool IsExternal(this MethodDefinition method) {
            return method.HasBody && method.RVA == 0;
        }

        public static TypeReference MakeGeneric(this TypeReference t, params TypeReference[] args) {
            if (!t.HasGenericParameters) {
                throw new ArgumentException("Type must have generic parameters");
            }
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

        public static TypeReference MakePointer(this TypeReference t) {
            return new PointerType(t);
        }

        public static MethodReference MakeGeneric(this MethodReference m, params TypeReference[] args) {
            if (!m.HasGenericParameters) {
                throw new ArgumentException("Method must have generic parameters");
            }
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
                var elType = ((ArrayType)type).ElementType;
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

        public static CustomAttribute GetCustomAttribute<TAttr>(this Mono.Cecil.ICustomAttributeProvider cap, bool incProperty = false) {
            var attr = cap.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == typeof(TAttr).Name);
            if (attr != null) {
                return attr;
            }
            if (incProperty) {
                var mRef = cap as MethodReference;
                if (mRef != null) {
                    var property = mRef.Resolve().DeclaringType.Properties
                        .FirstOrDefault(x => (x.SetMethod != null && x.SetMethod.Name == mRef.Name)
                            || (x.GetMethod != null && x.GetMethod.Name == mRef.Name));
                    if (property != null) {
                        return property.GetCustomAttribute<TAttr>(false);
                    }
                }
            }
            return null;
        }

        public static IEnumerable<CustomAttribute> GetCustomAttributes<TAttr>(this Mono.Cecil.ICustomAttributeProvider cap) {
            return cap.CustomAttributes.Where(x => x.AttributeType.Name == typeof(TAttr).Name).ToArray();
        }

        public static string Name(this TypeReference t) {
            var fullName = t.FullName;
            int depth = 0;
            for (int i = fullName.Length - 1; i >= 0; i--) {
                var c = fullName[i];
                if (c == '<') {
                    depth--;
                }
                if (c == '>') {
                    depth++;
                }
                if (depth == 0 && c == '.') {
                    return fullName.Substring(i + 1);
                }
            }
            return fullName;
        }

        public static TypeReference GetNullableInnerType(this TypeReference t) {
            return ((GenericInstanceType)t).GenericArguments[0];
        }

        public static TypeReference MakeNullable(this TypeReference t) {
            if (!t.IsValueType) {
                throw new ArgumentException("Nullable<T> must be value-type");
            }
            var nType = t.Module.Import(typeof(Nullable<>)).MakeGeneric(t);
            return nType;
        }

        private static string FullName(this TypeReference tRef) {
            if (tRef.IsArray) {
                var elementType = ((ArrayType)tRef).ElementType;
                return elementType.FullName() + "[]";
            }
            if (tRef.IsGenericInstance) {
                var tGen = (GenericInstanceType)tRef;
                var genAssemblyQualifiedNames = tGen.GenericArguments.Select(x => "[" + x.AssemblyQualifiedName() + "]").ToArray();
                var defFullName = tRef.Resolve().FullName.Replace('/', '+');
                var ret = string.Format("{0}[{1}]", defFullName, string.Join(",", genAssemblyQualifiedNames));
                return ret;
            }
            return tRef.FullName.Replace('/', '+');
        }

        public static string AssemblyQualifiedName(this TypeReference tRef) {
            var fullName = tRef.FullName();
            var assemblyName = ((Func<string>)(() => {
                switch (tRef.Scope.MetadataScopeType) {
                case MetadataScopeType.AssemblyNameReference:
                    return ((AssemblyNameReference)tRef.Scope).FullName;
                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition)tRef.Scope).Assembly.FullName;
                case MetadataScopeType.ModuleReference:
                    throw new InvalidOperationException("ModuleReference not handled");
                default:
                    throw new InvalidOperationException();
                }
            }))();
            var assemblyQualifiedName = fullName + ", " + assemblyName;
            return assemblyQualifiedName;
        }

        public static Type LoadType(this TypeReference tRef) {
            try {
                var name = tRef.AssemblyQualifiedName();
                var type = Type.GetType(name);
                return type;
            } catch (TypeLoadException) {
                return null;
            }
        }

        public static MethodBase LoadMethod(this MethodReference mRef) {
            var type = mRef.DeclaringType.LoadType();
            var mRefMDToken = mRef.Resolve().MetadataToken.ToInt32();
            type.Module.ResolveMethod(mRefMDToken);
            var methods = type.GetMethods();
            var ret = methods.First(x => x.MetadataToken == mRefMDToken);
            return ret;
        }

        public static TypeReference GetGenericArgument(this MethodReference mRef, int index) {
            return ((GenericInstanceMethod)mRef).GenericArguments[index];
        }

        public static TypeReference GetGenericArgument(this TypeReference tRef, int index) {
            return ((GenericInstanceType)tRef).GenericArguments[index];
        }

        private static bool ContainsGenericParameters(this TypeReference tRef, MethodReference scope) {
            var tRefResolved = tRef.FullResolve(scope != null ? scope.DeclaringType : null, scope, true);
            if (tRefResolved.IsArray) {
                return ((ArrayType)tRefResolved).ElementType.ContainsGenericParameters(scope);
            }
            if (tRefResolved.IsByReference) {
                return ((ByReferenceType)tRefResolved).ElementType.ContainsGenericParameters(scope);
            }
            if (tRefResolved.IsGenericParameter) {
                return true;
            }
            if (tRefResolved.HasGenericParameters) {
                return true;
            }
            if (tRefResolved.IsGenericInstance) {
                var tRefGenInst = (GenericInstanceType)tRefResolved;
                if (tRefGenInst.GenericArguments.Any(x => x.ContainsGenericParameters(scope))) {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsGenericParameters(this TypeReference tRef) {
            return tRef.ContainsGenericParameters(null);
        }

        public static bool ContainsGenericParameters(this MethodReference mRef) {
            if (mRef.DeclaringType.ContainsGenericParameters(mRef)) {
                return true;
            }
            if (mRef.HasGenericParameters) {
                return true;
            }
            if (mRef.IsGenericInstance) {
                var mRefGenInst = (GenericInstanceMethod)mRef;
                if (mRefGenInst.GenericArguments.Any(x => x.ContainsGenericParameters(mRef))) {
                    return true;
                }
            }
            if (mRef.ReturnType.ContainsGenericParameters(mRef)) {
                return true;
            }
            if (mRef.Parameters.Any(x => x.ParameterType.ContainsGenericParameters(mRef))) {
                return true;
            }
            return false;
        }

        public static TypeReference ElementType(this TypeReference self) {
            var asPointer = self as PointerType;
            if (asPointer != null) {
                return asPointer.ElementType;
            }
            var asArray = self as ArrayType;
            if (asArray != null) {
                return asArray.ElementType;
            }
            var asByRef = self as ByReferenceType;
            if (asByRef != null) {
                return asByRef.ElementType;
            }
            return self;
        }

    }
}
