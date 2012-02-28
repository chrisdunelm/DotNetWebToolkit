using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    static class _Enumerable {

        public static IEnumerable<T> Where<T>(this IEnumerable<T> e, Func<T, bool> predicate) {
            foreach (var item in e) {
                if (predicate(item)) {
                    yield return item;
                }
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> e, Func<TSource, TResult> selector) {
            foreach (var item in e) {
                yield return selector(item);
            }
        }

        public static int Count<T>(this IEnumerable<T> e) {
            var eCollection = e as ICollection;
            if (eCollection != null) {
                return eCollection.Count;
            }
            int count = 0;
            foreach (var item in e) {
                count++;
            }
            return count;
        }

        public static int Sum(this IEnumerable<int> e) {
            int sum = 0;
            foreach (var item in e) {
                sum += item;
            }
            return sum;
        }

        public static double Sum(this IEnumerable<double> e) {
            double sum = 0;
            foreach (var item in e) {
                sum += item;
            }
            return sum;
        }

        public static T[] ToArray<T>(this IEnumerable<T> e) {
            return new List<T>(e).ToArray();
        }

    }
}
