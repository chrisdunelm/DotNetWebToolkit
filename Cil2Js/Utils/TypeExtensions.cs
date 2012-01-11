using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Utils {
    public static class TypeExtensions {

        class TypeRefEqComparer : IEqualityComparer<TypeReference> {

            public bool Equals(TypeReference x, TypeReference y) {
                if (x.IsGenericParameter && y.IsGenericParameter) {
                    return true;
                }
                return x.FullName == y.FullName;
            }

            public int GetHashCode(TypeReference obj) {
                if (obj.IsGenericParameter) {
                    return 0;
                }
                return obj.FullName.GetHashCode();
            }

        }

        public static readonly IEqualityComparer<TypeReference> TypeRefEqComparerInstance = new TypeRefEqComparer();

        class MethodRefEqComparer : IEqualityComparer<MethodReference> {

            public bool Equals(MethodReference x, MethodReference y) {
                if (!x.MatchMethodOnly(y)) {
                    return false;
                }
                return TypeRefEqComparerInstance.Equals(x.DeclaringType, y.DeclaringType);
            }

            public int GetHashCode(MethodReference obj) {
                return obj.Name.GetHashCode() ^ TypeRefEqComparerInstance.GetHashCode(obj.DeclaringType);
            }

        }

        public static readonly IEqualityComparer<MethodReference> MethodRefEqComparerInstance = new MethodRefEqComparer();

        class FieldRefEqComparer : IEqualityComparer<FieldReference> {

            public bool Equals(FieldReference x, FieldReference y) {
                return x.FullName == y.FullName;
            }

            public int GetHashCode(FieldReference obj) {
                return obj.FullName.GetHashCode();
            }

        }

        public static readonly IEqualityComparer<FieldReference> FieldReqEqComparerInstance = new FieldRefEqComparer();

        class BaseFirstComparer : IComparer<TypeReference> {

            private bool IsABaseOfB(TypeReference a, TypeReference b) {
                var t = b;
                for (; ; ) {
                    t = t.GetBaseType();
                    if (t == null) {
                        return false;
                    }
                    if (t.FullName == a.FullName) {
                        return true;
                    }
                }
            }

            public int Compare(TypeReference x, TypeReference y) {
                if (this.IsABaseOfB(x, y)) {
                    return -1;
                }
                if (this.IsABaseOfB(y, x)) {
                    return 1;
                }
                return 0;
            }

        }

        public static readonly IComparer<TypeReference> BaseFirstComparerInstance = new BaseFirstComparer();

        //public static MethodDefinition GetBasemostMethodInTypeHierarchy(this MethodDefinition method) {
        //    var m = method;
        //    for (; ; ) {
        //        if (m.IsNewSlot) {
        //            return m;
        //        }
        //        m = m.GetBaseMethodInTypeHierarchy();
        //        if (m == null) {
        //            throw new InvalidOperationException("Error in type hierarchy");
        //        }
        //    }
        //}

        //public static MethodReference GetBasemostMethodInTypeHierarchy(this MethodReference method) {
        //    for (; ; ) {
        //        var mDef = method.Resolve();
        //        if (mDef.IsNewSlot) {
        //            return method;
        //        }
        //        method.DeclaringType.
        //    }
        //}

        //public static MethodDefinition GetBaseMethodInTypeHierarchy(this MethodDefinition method) {
        //    return GetBaseMethodInTypeHierarchy(method.DeclaringType, method);
        //}

        //static MethodDefinition GetBaseMethodInTypeHierarchy(TypeDefinition type, MethodDefinition method) {
        //    TypeDefinition @base = GetBaseType(type);
        //    while (@base != null) {
        //        MethodDefinition base_method = TryMatchMethod(@base, method);
        //        if (base_method != null)
        //            return base_method;

        //        @base = GetBaseType(@base);
        //    }

        //    return null;
        //}

        //public static MethodDefinition GetBaseMethodInInterfaceHierarchy(this MethodDefinition method) {
        //    return GetBaseMethodInInterfaceHierarchy(method.DeclaringType, method);
        //}

        //static MethodDefinition GetBaseMethodInInterfaceHierarchy(TypeDefinition type, MethodDefinition method) {
        //    if (!type.HasInterfaces)
        //        return null;

        //    foreach (TypeReference interface_ref in type.Interfaces) {
        //        TypeDefinition @interface = interface_ref.Resolve();
        //        if (@interface == null)
        //            continue;

        //        MethodDefinition base_method = TryMatchMethod(@interface, method);
        //        if (base_method != null)
        //            return base_method;

        //        base_method = GetBaseMethodInInterfaceHierarchy(@interface, method);
        //        if (base_method != null)
        //            return base_method;
        //    }

        //    return null;
        //}

        //public static MethodDefinition TryMatchMethod(this TypeDefinition type, MethodDefinition method) {
        //    if (!type.HasMethods)
        //        return null;

        //    foreach (MethodDefinition candidate in type.Methods)
        //        if (MethodMatch(candidate, method))
        //            return candidate;

        //    return null;
        //}

        public static bool MethodMatch(this MethodDefinition candidate, MethodDefinition method) {
            if (!candidate.IsVirtual)
                return false;

            if (candidate.Name != method.Name)
                return false;

            if (!TypeMatch(candidate.ReturnType, method.ReturnType))
                return false;

            if (candidate.Parameters.Count != method.Parameters.Count)
                return false;

            for (int i = 0; i < candidate.Parameters.Count; i++)
                if (!TypeMatch(candidate.Parameters[i].ParameterType, method.Parameters[i].ParameterType))
                    return false;

            return true;
        }

        static bool TypeMatch(IModifierType a, IModifierType b) {
            if (!TypeMatch(a.ModifierType, b.ModifierType))
                return false;

            return TypeMatch(a.ElementType, b.ElementType);
        }

        static bool TypeMatch(TypeSpecification a, TypeSpecification b) {
            if (a is GenericInstanceType)
                return TypeMatch((GenericInstanceType)a, (GenericInstanceType)b);

            if (a is IModifierType)
                return TypeMatch((IModifierType)a, (IModifierType)b);

            return TypeMatch(a.ElementType, b.ElementType);
        }

        static bool TypeMatch(GenericInstanceType a, GenericInstanceType b) {
            if (!TypeMatch(a.ElementType, b.ElementType))
                return false;

            if (a.GenericArguments.Count != b.GenericArguments.Count)
                return false;

            if (a.GenericArguments.Count == 0)
                return true;

            for (int i = 0; i < a.GenericArguments.Count; i++)
                if (!TypeMatch(a.GenericArguments[i], b.GenericArguments[i]))
                    return false;

            return true;
        }

        static bool TypeMatch(TypeReference a, TypeReference b) {
            if (a is GenericParameter)
                return true;

            if (a is TypeSpecification || b is TypeSpecification) {
                if (a.GetType() != b.GetType())
                    return false;

                return TypeMatch((TypeSpecification)a, (TypeSpecification)b);
            }

            return a.FullName == b.FullName;
        }

        public static TypeDefinition GetBaseType(this TypeDefinition type) {
            if (type == null || type.BaseType == null)
                return null;

            return type.BaseType.Resolve();
        }

        public static bool DoesImplement(this TypeReference t, TypeReference iFace) {
            return t.Resolve().Interfaces.Any(x => TypeMatch(x, iFace));
        }

        public static bool IsAssignableTo(this TypeReference from, TypeReference to) {
            // Rules from ECMA-335 partition III page 21
            // Rule 4
            if (to.FullName == "System.Object") {
                return true;
            }
            // Rule 1
            if (TypeMatch(from, to)) {
                return true;
            }
            // Rule 7
            if (from.IsArray && to.IsArray) {
                return from.GetElementType().IsAssignableTo(to.GetElementType());
            }
            // Rule 3
            var baseFrom = from.Resolve().GetBaseType();
            while (baseFrom != null) {
                if (TypeMatch(baseFrom, to)) {
                    return true;
                }
                baseFrom = baseFrom.GetBaseType();
            }
            var toDef = to.Resolve();
            if (toDef.IsInterface) {
                if (from.DoesImplement(toDef)) {
                    return true;
                }
            }
            // TODO: Other rules

            return false;
        }

        /// <summary>
        /// Returns the method in 'type' that implements the interface method 'iFaceMethod'
        /// </summary>
        /// <param name="type"></param>
        /// <param name="iFaceMethod"></param>
        /// <returns></returns>
        public static MethodDefinition GetInterfaceMethod(this TypeDefinition type, MethodDefinition iFaceMethod) {
            return type.EnumThisAndBases().SelectMany(x => x.Methods).First(m => {
                // Explicit implementation
                if (m.Overrides.Any(x => x.FullName == iFaceMethod.FullName)) {
                    return true;
                }
                // Implicit implementation
                return m.MethodMatch(iFaceMethod);
            });
        }

        public static IEnumerable<TypeDefinition> EnumThisAndBases(this TypeDefinition type) {
            for (; ; ) {
                yield return type;
                type = type.GetBaseType();
                if (type == null) {
                    break;
                }
            }
        }

        /// <summary>
        /// Returns all interfaces that 'type' implements, including inheritance
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<TypeDefinition> GetAllInterfaces(this TypeDefinition type) {
            IEnumerable<TypeDefinition> allInterfaces = Enumerable.Empty<TypeDefinition>();
            foreach (var t in type.EnumThisAndBases()) {
                allInterfaces = allInterfaces.Union(t.Interfaces.Select(x => x.Resolve()));
            }
            return allInterfaces;
        }

    }
}
