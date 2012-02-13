using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    static class _Enumerable {

        public static IEnumerable<T> Where<T>(this IEnumerable<T> e, Func<T, bool> predicate) {
            //var z = e.GetEnumerator();
            //while (z.MoveNext()) {
            //    yield return z.Current;
            //}
            foreach (var item in e) {
                if (predicate(item)) {
                    yield return item;
                }
            }
        }

        public static int Count<T>(this IEnumerable<T> e) {
            var eCollection = e as ICollection;
            if (e != null) {
                return eCollection.Count;
            }
            int count = 0;
            foreach (var item in e) {
                count++;
            }
            return count;
        }

    }
}
