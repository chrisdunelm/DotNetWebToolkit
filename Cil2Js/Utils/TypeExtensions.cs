using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (b.IsAssignableTo(a)) {
                return true;
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
            // TODO: Make this more efficient than this insertion sort
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

        public static bool IsSingle(this TypeReference type) {
            return type.MetadataType == MetadataType.Single;
        }

        public static bool IsDouble(this TypeReference type) {
            return type.MetadataType == MetadataType.Double;
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

        public static bool IsNullable(this TypeReference type) {
            return type.FullName.StartsWith("System.Nullable`1");
        }

        public static bool IsInteger(this TypeReference type) {
            return
                type.IsByte() || type.IsSByte() ||
                type.IsInt16() || type.IsUInt16() ||
                type.IsInt32() || type.IsUInt32() ||
                type.IsInt64() || type.IsUInt64();
        }

        public static bool IsNumeric(this TypeReference type) {
            return type.IsInteger() || type.IsSingle() || type.IsDouble();
        }

        public static bool IsSignedInteger(this TypeReference type) {
            return type.IsSByte() || type.IsInt16() || type.IsInt32() || type.IsInt64();
        }

        public static bool IsUnsignedInteger(this TypeReference type) {
            return type.IsByte() || type.IsUInt16() || type.IsUInt32() || type.IsUInt64();
        }

        public static bool IsBaseOfOrEqual(this TypeReference less, TypeReference more) {
            var t = more;
            do {
                if (t.IsSame(less)) {
                    return true;
                }
                t = t.GetBaseType();
            } while (t != null);
            return false;
        }

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

        public static Type ReturnType(this MemberInfo mi) {
            switch (mi.MemberType) {
            case MemberTypes.Constructor:
                return typeof(void);
            case MemberTypes.Method:
                return ((MethodInfo)mi).ReturnType;
            default:
                throw new NotImplementedException("Cannot handle: " + mi.MemberType);
            }
        }

    }
}
