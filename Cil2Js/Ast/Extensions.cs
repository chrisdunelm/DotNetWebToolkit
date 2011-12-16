using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Utils;

namespace Cil2Js.Ast {
    public static class Extensions {

        public static bool IsSame(this TypeReference type, TypeReference other) {
            return type.FullName == other.FullName;
        }

        public static bool IsVoid(this TypeReference type) {
            return type.FullName == "System.Void";
        }

        public static bool IsObject(this TypeReference type) {
            return type.FullName == "System.Object";
        }

        public static bool IsBoolean(this TypeReference type) {
            return type.FullName == "System.Boolean";
        }

        public static bool IsInt32(this TypeReference type) {
            return type.FullName == "System.Int32";
        }

        public static bool IsString(this TypeReference type) {
            return type.FullName == "System.String";
        }

        public static bool IsChar(this TypeReference type) {
            return type.FullName == "System.Char";
        }

        public static bool IsException(this TypeReference type) {
            return type.FullName == "System.Exception";
        }

        public static bool DoesEqual(this Expr a, Expr b) {
            return VisitorSameExpr.AreSame(a, b);
        }

        public static bool DoesEqualExact(this Expr a, Expr b) {
            return VisitorSameExpr.AreSame(a, b, true);
        }

        private static IEnumerable<T[]> Permutations<T>(T[] set) {
            if (set.Length == 1) {
                yield return set;
            } else {
                for (int i = 0; i < set.Length; i++) {
                    T first = set[i];
                    T[] rest = set.Take(i).Concat(set.Skip(i + 1)).ToArray();
                    foreach (var r in Permutations(rest)) {
                        yield return new[] { first }.Concat(r).ToArray();
                    }
                }
            }
        }

        public static TResult Perms<T, TResult>(this Tuple<T, T> ab, Func<T, T, TResult> fn) {
            foreach (var permutation in Permutations(new[] { ab.Item1, ab.Item2 })) {
                var ret = fn(permutation[0], permutation[1]);
                if (!ret.IsDefault()) {
                    return ret;
                }
            }
            return default(TResult);
        }

        public static TResult Perms<T, TResult>(this Tuple<T, T, T> abc, Func<T, T, T, TResult> fn) {
            foreach (var permutation in Permutations(new[] { abc.Item1, abc.Item2, abc.Item3 })) {
                var ret = fn(permutation[0], permutation[1], permutation[2]);
                if (!ret.IsDefault()) {
                    return ret;
                }
            }
            return default(TResult);
        }

        public static bool DoesEqualNot(this Expr a, Expr b) {
            return Tuple.Create(a, b).Perms((_a, _b) => {
                if (_a.ExprType == Expr.NodeType.Unary) {
                    var aUn = (ExprUnary)_a;
                    if (aUn.Op == UnaryOp.Not && VisitorSameExpr.AreSame(aUn.Expr, _b)) {
                        return true;
                    }
                }
                return false;
            });
        }

        public static bool IsLiteralBoolean(this Expr e, bool value) {
            if (e.ExprType == Expr.NodeType.Literal) {
                var eLit = (ExprLiteral)e;
                if (eLit.Type.IsBoolean()) {
                    return value == (bool)eLit.Value;
                }
                if (eLit.Type.IsInt32()) {
                    return value == ((int)eLit.Value != 0 ? true : false);
                }
                throw new InvalidOperationException("This literal cannot be treated like a boolean");
            }
            return false;
        }

        public static bool DoesEqual(this Stmt a, Stmt b) {
            return VisitorSameStmt.AreSame(a, b);
        }

    }
}
