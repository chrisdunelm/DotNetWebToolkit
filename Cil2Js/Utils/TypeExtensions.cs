using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Utils {
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

        private static bool IsABeforeB(TypeReference a, TypeReference b) {
            // Interfaces always come first
            var aDef  =a.Resolve();
            var bDef=b.Resolve();
            if (aDef.IsInterface && !bDef.IsInterface) {
                return true;
            }
            // Check base-type
            var t = b.GetBaseType();
            while (t != null) {
                if (t.IsSame(a)) {
                    // a is a base-type of b
                    return true;
                }
                t = t.GetBaseType();
            }
            // Check array element-type
            if (b.IsArray) {
                var bElType = b.GetElementType();
                if (bElType.IsSame(a)) {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<T> OrderByReferencedFirst<T>(this IEnumerable<T> en, Func<T, TypeReference> selector) {
            // TODO: Make this more efficient.
            // Cannot use built-in sort/orderby, as set is only partially ordered
            var ret = new List<T>();
            foreach (var item in en) {
                bool inserted = false;
                for (int i = 0; i < ret.Count; i++) {
                    if (IsABeforeB(selector(item), selector(ret[i]))) {
                        ret.Insert(i, item);
                        inserted = true;
                        break;
                    }
                }
                if (!inserted) {
                    ret.Add(item);
                }
            }
            return ret;
        }

        public static bool IsSame(this TypeReference type, TypeReference other) {
            return TypeExtensions.TypeRefEqComparerInstance.Equals(type, other);
        }

        public static bool IsVoid(this TypeReference type) {
            return type.MetadataType == MetadataType.Void;
        }

        public static bool IsObject(this TypeReference type) {
            return type.MetadataType == MetadataType.Object;
        }

        public static bool IsBoolean(this TypeReference type) {
            return type.MetadataType == MetadataType.Boolean;
        }

        public static bool IsByte(this TypeReference type) {
            return type.MetadataType == MetadataType.Byte;
        }

        public static bool IsSByte(this TypeReference type) {
            return type.MetadataType == MetadataType.SByte;
        }

        public static bool IsInt16(this TypeReference type) {
            return type.MetadataType == MetadataType.Int16;
        }

        public static bool IsUInt16(this TypeReference type) {
            return type.MetadataType == MetadataType.UInt16;
        }

        public static bool IsInt32(this TypeReference type) {
            return type.MetadataType == MetadataType.Int32;
        }

        public static bool IsUInt32(this TypeReference type) {
            return type.MetadataType == MetadataType.UInt32;
        }

        public static bool IsInt64(this TypeReference type) {
            return type.MetadataType == MetadataType.Int64;
        }

        public static bool IsUInt64(this TypeReference type) {
            return type.MetadataType == MetadataType.UInt64;
        }

        public static bool IsString(this TypeReference type) {
            return type.MetadataType == MetadataType.String;
        }

        public static bool IsChar(this TypeReference type) {
            return type.MetadataType == MetadataType.Char;
        }

        public static bool IsException(this TypeReference type) {
            return type.FullName == "System.Exception";
        }

        public static bool IsInteger(this TypeReference type) {
            return
                type.IsByte() || type.IsSByte() ||
                type.IsInt16() || type.IsUInt16() ||
                type.IsInt32() || type.IsUInt32() ||
                type.IsInt64() || type.IsUInt64();
        }

        //public static bool MethodMatch(this MethodDefinition candidate, MethodDefinition method) {
        //    if (!candidate.IsVirtual)
        //        return false;

        //    if (candidate.Name != method.Name)
        //        return false;

        //    if (!TypeMatch(candidate.ReturnType, method.ReturnType))
        //        return false;

        //    if (candidate.Parameters.Count != method.Parameters.Count)
        //        return false;

        //    for (int i = 0; i < candidate.Parameters.Count; i++)
        //        if (!TypeMatch(candidate.Parameters[i].ParameterType, method.Parameters[i].ParameterType))
        //            return false;

        //    return true;
        //}

        //static bool TypeMatch(IModifierType a, IModifierType b) {
        //    if (!TypeMatch(a.ModifierType, b.ModifierType))
        //        return false;

        //    return TypeMatch(a.ElementType, b.ElementType);
        //}

        //static bool TypeMatch(TypeSpecification a, TypeSpecification b) {
        //    if (a is GenericInstanceType)
        //        return TypeMatch((GenericInstanceType)a, (GenericInstanceType)b);

        //    if (a is IModifierType)
        //        return TypeMatch((IModifierType)a, (IModifierType)b);

        //    return TypeMatch(a.ElementType, b.ElementType);
        //}

        //static bool TypeMatch(GenericInstanceType a, GenericInstanceType b) {
        //    if (!TypeMatch(a.ElementType, b.ElementType))
        //        return false;

        //    if (a.GenericArguments.Count != b.GenericArguments.Count)
        //        return false;

        //    if (a.GenericArguments.Count == 0)
        //        return true;

        //    for (int i = 0; i < a.GenericArguments.Count; i++)
        //        if (!TypeMatch(a.GenericArguments[i], b.GenericArguments[i]))
        //            return false;

        //    return true;
        //}

        //static bool TypeMatch(TypeReference a, TypeReference b) {
        //    if (a is GenericParameter)
        //        return true;

        //    if (a is TypeSpecification || b is TypeSpecification) {
        //        if (a.GetType() != b.GetType())
        //            return false;

        //        return TypeMatch((TypeSpecification)a, (TypeSpecification)b);
        //    }

        //    return a.FullName == b.FullName;
        //}

        //public static TypeDefinition GetBaseType(this TypeDefinition type) {
        //    if (type == null || type.BaseType == null)
        //        return null;

        //    return type.BaseType.Resolve();
        //}

        public static bool IsAssignableTo(this TypeReference from, TypeReference to) {
            // Rules from ECMA-335 partition III page 21
            // Rule 4
            if (to.FullName == "System.Object") {
                return true;
            }
            // Rule 1
            if (from.IsSame(to)) {
                return true;
            }
            // Rule 7
            if (from.IsArray && to.IsArray) {
                return from.GetElementType().IsAssignableTo(to.GetElementType());
            }
            // Rule 3
            var baseFrom = from.GetBaseType();
            while (baseFrom != null) {
                if (baseFrom.IsSame(to)) {
                    return true;
                }
                baseFrom = baseFrom.GetBaseType();
            }
            var toDef = to.Resolve();
            if (toDef.IsInterface) {
                if (from.DoesImplement(to)) {
                    return true;
                }
            }
            // TODO: Other rules

            return false;
        }

    }
}
