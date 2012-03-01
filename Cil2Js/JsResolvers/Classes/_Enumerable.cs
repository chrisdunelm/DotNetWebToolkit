using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    static class _Enumerable {

        #region Count

        public static int Count<TSource>(this IEnumerable<TSource> source) {
            var sourceCollection = source as ICollection;
            if (sourceCollection != null) {
                return sourceCollection.Count;
            }
            int count = 0;
            foreach (var item in source) {
                count++;
            }
            return count;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            int count = 0;
            foreach (var item in source) {
                if (predicate(item)) {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region Select

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            foreach (var item in source) {
                yield return selector(item);
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
            int i = 0;
            foreach (var item in source) {
                yield return selector(item, i);
                i++;
            }
        }

        #endregion Select

        #region Sum

        public static int Sum(this IEnumerable<int> source) {
            int sum = 0;
            foreach (var item in source) {
                sum += item;
            }
            return sum;
        }

        public static double Sum(this IEnumerable<double> source) {
            double sum = 0;
            foreach (var item in source) {
                sum += item;
            }
            return sum;
        }

        #endregion

        #region Where

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            foreach (var item in source) {
                if (predicate(item)) {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, int, bool> predicate) {
            int i = 0;
            foreach (var item in source) {
                if (predicate(item, i)) {
                    yield return item;
                }
                i++;
            }
        }

        #endregion

        #region Converters

        public static T[] ToArray<T>(this IEnumerable<T> source) {
            return new List<T>(source).ToArray();
        }

        public static List<T> ToList<T>(this IEnumerable<T> source) {
            return new List<T>(source);
        }

        #endregion

    }
}
