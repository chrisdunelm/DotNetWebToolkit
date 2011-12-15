using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Utils {
    public static class TypeExtensions {

        class BaseFirstComparer : IComparer<TypeDefinition> {

            public static IComparer<TypeDefinition> Instance = new BaseFirstComparer();

            private BaseFirstComparer() { }

            private bool IsABaseOfB(TypeDefinition a, TypeDefinition b) {
                var t = b;
                for (; ; ) {
                    t = t.GetBaseType();
                    if (t == null) {
                        return false;
                    }
                    if (t == a) {
                        return true;
                    }
                }
            }

            public int Compare(TypeDefinition x, TypeDefinition y) {
                // Return less than zero if x is more base than y
                if (this.IsABaseOfB(x, y)) {
                    return -1;
                }
                if (this.IsABaseOfB(y, x)) {
                    return 1;
                }
                return 0;
            }

        }

        public static IEnumerable<TypeDefinition> OrderByBaseFirst(this IEnumerable<TypeDefinition> types) {
            return types.OrderBy(x => x, BaseFirstComparer.Instance);
        }

        public static MethodDefinition GetBasemostMethodInTypeHierarchy(this MethodDefinition method) {
            var m = method;
            for (; ; ) {
                if (m.IsNewSlot) {
                    return m;
                }
                m = m.GetBaseMethodInTypeHierarchy();
                if (m == null) {
                    throw new InvalidOperationException("Error in type hierarchy");
                }
            }
        }

        public static MethodDefinition GetBaseMethodInTypeHierarchy(this MethodDefinition method) {
            return GetBaseMethodInTypeHierarchy(method.DeclaringType, method);
        }

        static MethodDefinition GetBaseMethodInTypeHierarchy(TypeDefinition type, MethodDefinition method) {
            TypeDefinition @base = GetBaseType(type);
            while (@base != null) {
                MethodDefinition base_method = TryMatchMethod(@base, method);
                if (base_method != null)
                    return base_method;

                @base = GetBaseType(@base);
            }

            return null;
        }

        public static MethodDefinition GetBaseMethodInInterfaceHierarchy(this MethodDefinition method) {
            return GetBaseMethodInInterfaceHierarchy(method.DeclaringType, method);
        }

        static MethodDefinition GetBaseMethodInInterfaceHierarchy(TypeDefinition type, MethodDefinition method) {
            if (!type.HasInterfaces)
                return null;

            foreach (TypeReference interface_ref in type.Interfaces) {
                TypeDefinition @interface = interface_ref.Resolve();
                if (@interface == null)
                    continue;

                MethodDefinition base_method = TryMatchMethod(@interface, method);
                if (base_method != null)
                    return base_method;

                base_method = GetBaseMethodInInterfaceHierarchy(@interface, method);
                if (base_method != null)
                    return base_method;
            }

            return null;
        }

        public static MethodDefinition TryMatchMethod(this TypeDefinition type, MethodDefinition method) {
            if (!type.HasMethods)
                return null;

            foreach (MethodDefinition candidate in type.Methods)
                if (MethodMatch(candidate, method))
                    return candidate;

            return null;
        }

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
            return type.Methods.First(m => {
                // Explicit implementation
                if (m.Overrides.Any(x => x.FullName == iFaceMethod.FullName)) {
                    return true;
                }
                // Implicit implementation
                return m.MethodMatch(iFaceMethod);
            });
        }

    }
}
