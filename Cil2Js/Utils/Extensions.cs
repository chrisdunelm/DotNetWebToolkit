using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace Cil2Js.Utils {
    public static class Extensions {

        [DebuggerStepThrough]
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> en, T item) {
            foreach (var e in en) yield return e;
            yield return item;
        }

        [DebuggerStepThrough]
        public static IEnumerable<Instruction> GetRange(this Instruction start, Instruction end) {
            if (start.Offset > end.Offset) {
                throw new ArgumentException();
            }
            var ret = new List<Instruction>();
            var inst = start;
            for (; ; ) {
                ret.Add(inst);
                if (inst == end) {
                    break;
                }
                inst = inst.Next;
            }
            return ret;
        }

        [DebuggerStepThrough]
        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key) {
            TValue value;
            return d.TryGetValue(key, out value) ? value : default(TValue);
        }

        [DebuggerStepThrough]
        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, Func<TValue> fnDefault, bool autoAdd = false) {
            TValue value;
            if (!d.TryGetValue(key, out value)) {
                value = fnDefault();
                if (autoAdd) {
                    d.Add(key, value);
                }
            }
            return value;
        }

        [DebuggerStepThrough]
        public static bool IsDefault<T>(this T o) {
            return object.Equals(o, default(T));
        }

        [DebuggerStepThrough]
        public static TResult NullThru<T, TResult>(this T o, Func<T, TResult> fn, TResult @default = default(TResult)) where T : class {
            return o != null ? fn(o) : @default;
        }

        struct CombineResult<T> {
            public bool hasValue;
            public T value;
        }
        private static IEnumerable<T> Combine<T>(IEnumerable<T> en, Func<T, T, CombineResult<T>> fnCombine) {
            bool havePrev = false;
            T prev = default(T);
            foreach (var item in en) {
                if (havePrev) {
                    var combined = fnCombine(prev, item);
                    if (combined.hasValue) {
                        prev = combined.value;
                    } else {
                        yield return prev;
                        prev = item;
                    }
                } else {
                    prev = item;
                }
                havePrev = true;
            }
            if (havePrev) {
                yield return prev;
            }
        }

        [DebuggerStepThrough]
        public static IEnumerable<T> Combine<T>(this IEnumerable<T> en, Func<T, T, T> fnCombine) where T : class {
            return Combine(en, (a, b) => {
                var o = fnCombine(a, b);
                return new CombineResult<T> {
                    hasValue = o != null,
                    value = o
                };
            });
        }

        [DebuggerStepThrough]
        public static IEnumerable<T> Combine<T>(this IEnumerable<T> en, Func<T, T, T?> fnCombine) where T : struct {
            return Combine(en, (a, b) => {
                var o = fnCombine(a, b);
                return new CombineResult<T> {
                    hasValue = o.HasValue,
                    value = o.GetValueOrDefault()
                };
            });
        }

    }
}
