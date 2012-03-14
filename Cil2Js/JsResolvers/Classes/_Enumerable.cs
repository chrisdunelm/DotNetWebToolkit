using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    #region Ordering classes

    class OrderedEnumerableItem<TElement> {
        public TElement item;
        public bool newSubOrder;
    }

    class OrderedEnumerable<TElement, TKey> : IOrderedEnumerable<TElement>, IEnumerable<TElement>, IEnumerable {

        class ItemWithKey : OrderedEnumerableItem<TElement> {
            public TKey key;
        }

        class Comparer : IComparer<ItemWithKey> {

            internal Comparer(IComparer<TKey> comparer, bool isDescending) {
                this.comparer = comparer;
                this.mult = isDescending ? -1 : 1;
            }

            private IComparer<TKey> comparer;
            private int mult;

            public int Compare(ItemWithKey x, ItemWithKey y) {
                return this.comparer.Compare(x.key, y.key) * this.mult;
            }

        }

        internal OrderedEnumerable(IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool isDescending)
            : this(source.Select((x, i) => new OrderedEnumerableItem<TElement> { item = x, newSubOrder = i == 0 }), keySelector, comparer, isDescending) {
        }

        private OrderedEnumerable(IEnumerable<OrderedEnumerableItem<TElement>> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool isDescending) {
            this.source = source;
            this.keySelector = keySelector;
            if (comparer == null) {
                comparer = Comparer<TKey>.Default;
            }
            this.keyComparer = comparer;
            this.comparer = new Comparer(comparer, isDescending);
        }

        private IEnumerable<OrderedEnumerableItem<TElement>> source;
        private Func<TElement, TKey> keySelector;
        private IComparer<TKey> keyComparer;
        private Comparer comparer;

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TSubKey>(Func<TElement, TSubKey> keySelector, IComparer<TSubKey> comparer, bool descending) {
            return new OrderedEnumerable<TElement, TSubKey>(this.GetElementEnumerator(), keySelector, comparer, descending);
        }

        private IEnumerable<OrderedEnumerableItem<TElement>> GetElementEnumerator() {
            var list = new List<ItemWithKey>();
            var en = this.source.GetEnumerator();
            var current = default(OrderedEnumerableItem<TElement>);
            var prevKey = default(TKey);
            for (; ; ) {
                var valid = en.MoveNext();
                if (valid) {
                    current = en.Current;
                }
                if (current.newSubOrder || !valid) {
                    var count = list.Count;
                    if (count > 1) {
                        list.Sort(this.comparer);
                    }
                    for (int i = 0; i < count; i++) {
                        list[i].newSubOrder = i == 0 || this.keyComparer.Compare(list[i].key, list[i - 1].key) != 0; // Note: relies on lazy boolean evaluation
                        yield return list[i];
                    }
                    list.Clear();
                    if (!valid) {
                        break;
                    }
                }
                var key = this.keySelector(current.item);
                list.Add(new ItemWithKey {
                    item = current.item,
                    key = key,
                });
                prevKey = key;
            }
        }

        public IEnumerator<TElement> GetEnumerator() {
            return this.GetElementEnumerator().Select(x => x.item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }

    #endregion

    #region Lookup classes

    class Lookup<TKey, TElement> : ILookup<TKey, TElement> {

        internal Lookup(Dictionary<TKey, List<TElement>> data) {
            this.data = data;
        }

        private Dictionary<TKey, List<TElement>> data;

        public bool Contains(TKey key) {
            return this.data.ContainsKey(key);
        }

        public int Count {
            get { return this.data.Count; }
        }

        public IEnumerable<TElement> this[TKey key] {
            get {
                List<TElement> value;
                if (this.data.TryGetValue(key, out value)) {
                    return value;
                } else {
                    return Enumerable.Empty<TElement>();
                }
            }
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
            foreach (var item in this.data) {
                yield return new Grouping<TKey, TElement>(item.Key, item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

    }

    #endregion

    #region Grouping classes

    class Grouping<TKey, TElement> : IGrouping<TKey, TElement> {

        internal Grouping(TKey key, IEnumerable<TElement> elements) {
            this.key = key;
            this.elements = elements;
        }

        private TKey key;
        private IEnumerable<TElement> elements;

        public TKey Key {
            get { return this.key; }
        }

        public IEnumerator<TElement> GetEnumerator() {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.elements.GetEnumerator();
        }

    }

    #endregion

    static class _Enumerable {

        #region Aggregate

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) {
            var acc = default(TSource);
            var first = true;
            foreach (var item in source) {
                if (first) {
                    acc = item;
                    first = false;
                } else {
                    acc = func(acc, item);
                }
            }
            if (first) {
                throw new InvalidOperationException();
            }
            return acc;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) {
            var acc = seed;
            foreach (var item in source) {
                acc = func(acc, item);
            }
            return acc;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) {
            return resultSelector(source.Aggregate(seed, func));
        }

        #endregion

        #region Any, All

        public static bool Any<TSource>(this IEnumerable<TSource> source) {
            var col = source as ICollection;
            if (col != null) {
                return col.Count > 0;
            }
            var colT = source as ICollection<TSource>;
            if (colT != null) {
                return colT.Count > 0;
            }
            using (var en = source.GetEnumerator()) {
                return en.MoveNext();
            }
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source) {
                if (predicate(item)) {
                    return true;
                }
            }
            return false;
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source) {
                if (!predicate(item)) {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Concat

        class ConcatEnumerator<T> : IEnumerable<T>, IEnumerator<T> {

            internal ConcatEnumerator(IEnumerable<T> first, IEnumerable<T> second, bool isEnumerator) {
                this.first = first;
                this.second = second;
                if (isEnumerator) {
                    this.en1 = first.GetEnumerator();
                }
            }

            private IEnumerable<T> first, second;
            private IEnumerator<T> en1, en2;
            private T current;

            public IEnumerator<T> GetEnumerator() {
                return new ConcatEnumerator<T>(this.first, this.second, true);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new ConcatEnumerator<T>(this.first, this.second, true);
            }

            public T Current {
                get { return this.current; }
            }

            public void Dispose() {
                if (this.en1 != null) {
                    this.en1.Dispose();
                    this.en1 = null;
                }
                if (this.en2 != null) {
                    this.en2.Dispose();
                    this.en2 = null;
                }
            }

            object IEnumerator.Current {
                get { return this.current; }
            }

            public bool MoveNext() {
                if (this.en1 != null) {
                    if (this.en1.MoveNext()) {
                        this.current = this.en1.Current;
                        return true;
                    } else {
                        this.en1.Dispose();
                        this.en1 = null;
                        this.en2 = this.second.GetEnumerator();
                    }
                }
                if (this.en2 != null) {
                    if (this.en2.MoveNext()) {
                        this.current = this.en2.Current;
                        return true;
                    } else {
                        this.en2.Dispose();
                        this.en2 = null;
                    }
                }
                return false;
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            return new ConcatEnumerator<TSource>(first, second, false);
            //foreach (var item in first) {
            //    yield return item;
            //}
            //foreach (var item in second) {
            //    yield return item;
            //}
        }

        #endregion

        #region Count

        public static int Count<TSource>(this IEnumerable<TSource> source) {
            var sourceCollection = source as ICollection;
            if (sourceCollection != null) {
                return sourceCollection.Count;
            }
            var sourceCollectionT = source as ICollection<TSource>;
            if (sourceCollectionT != null) {
                return sourceCollectionT.Count;
            }
            int count = 0;
            using (var en = source.GetEnumerator()) {
                while (en.MoveNext()) {
                    count++;
                }
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

        #region Distinct

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) {
            return source.Distinct(null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            var hs = new HashSet<TSource>(comparer);
            foreach (var item in source) {
                if (hs.Add(item)) {
                    yield return item;
                }
            }
        }

        #endregion

        #region First, FirstOrDefault

        public static TSource First<TSource>(this IEnumerable<TSource> source) {
            foreach (var item in source) {
                return item;
            }
            throw new InvalidOperationException();
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source) {
                if (predicate(item)) {
                    return item;
                }
            }
            throw new InvalidOperationException();
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) {
            foreach (var item in source) {
                return item;
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source) {
                if (predicate(item)) {
                    return item;
                }
            }
            return default(TSource);
        }

        #endregion

        #region GroupBy

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return source.ToLookup(keySelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return source.ToLookup(keySelector, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            return source.ToLookup(keySelector, elementSelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            return source.ToLookup(keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector) {
            return source.ToLookup(keySelector, null).Select(x => resultSelector(x.Key, x));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            return source.ToLookup(keySelector, comparer).Select(x => resultSelector(x.Key, x));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector) {
            return source.ToLookup(keySelector, elementSelector, null).Select(x => resultSelector(x.Key, x));
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            return source.ToLookup(keySelector, elementSelector, comparer).Select(x => resultSelector(x.Key, x));
        }

        #endregion

        #region GroupJoin

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>
            (this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) {
            return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>
            (this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            var innerLookup = inner.ToLookup(innerKeySelector, comparer);
            foreach (var outerItem in outer) {
                var outerKey = outerKeySelector(outerItem);
                yield return resultSelector(outerItem, innerLookup[outerKey]);
            }
        }

        #endregion

        #region Join

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>
            (this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector) {
            return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>
            (this IEnumerable<TOuter> outer, IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            var innerLookup = inner.ToLookup(innerKeySelector, comparer);
            foreach (var outerItem in outer) {
                var outerKey = outerKeySelector(outerItem);
                foreach (var innerItem in innerLookup[outerKey]) {
                    yield return resultSelector(outerItem, innerItem);
                }
            }
        }

        #endregion

        #region Last, LastOrDefault

        public static TSource Last<TSource>(this IEnumerable<TSource> source) {
            var list = source as IList<TSource>;
            if (list != null) {
                if (list.Count > 0) {
                    return list[list.Count - 1];
                }
            } else {
                TSource last = default(TSource);
                bool any = false;
                foreach (var item in source) {
                    last = item;
                    any = true;
                }
                if (any) {
                    return last;
                }
            }
            throw new InvalidOperationException();
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            TSource last = default(TSource);
            bool any = false;
            foreach (var item in source) {
                if (predicate(item)) {
                    last = item;
                    any = true;
                }
            }
            if (any) {
                return last;
            }
            throw new InvalidOperationException();
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) {
            var list = source as IList<TSource>;
            TSource last = default(TSource);
            if (list != null) {
                if (list.Count > 0) {
                    last = list[list.Count - 1];
                }
            } else {
                foreach (var item in source) {
                    last = item;
                }
            }
            return last;
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            TSource last = default(TSource);
            foreach (var item in source) {
                if (predicate(item)) {
                    last = item;
                }
            }
            return last;
        }

        #endregion

        #region OrderBy, OrderByDescending, ThenBy, ThenByDescending

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return source.CreateOrderedEnumerable(keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return source.CreateOrderedEnumerable(keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return source.CreateOrderedEnumerable(keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return source.CreateOrderedEnumerable(keySelector, comparer, true);
        }

        #endregion

        #region Reverse

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source) {
            var list = source.ToList();
            list.Reverse();
            return list;
        }

        #endregion

        #region Select

        class SelectEnumerator<TSource, TResult> : IEnumerable<TResult>, IEnumerator<TResult> {

            internal SelectEnumerator(IEnumerable<TSource> source, Func<TSource, TResult> selector1, Func<TSource, int, TResult> selector2) {
                this.source = source;
                this.selector1 = selector1;
                this.selector2 = selector2;
            }

            private IEnumerable<TSource> source;
            private Func<TSource, TResult> selector1;
            private Func<TSource, int, TResult> selector2;
            private IEnumerator<TSource> en;
            private TResult current;
            private int count;

            public IEnumerator<TResult> GetEnumerator() {
                return new SelectEnumerator<TSource, TResult>(this.source, this.selector1, this.selector2);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            public TResult Current {
                get { return this.current; }
            }

            public void Dispose() {
                if (this.en != null) {
                    this.en.Dispose();
                }
            }

            object IEnumerator.Current {
                get { return this.current; }
            }

            public bool MoveNext() {
                if (this.en == null) {
                    this.en = this.source.GetEnumerator();
                    this.count = -1;
                }
                var ret = this.en.MoveNext();
                if (ret) {
                    this.count++;
                    var cur = this.en.Current;
                    this.current = this.selector1 != null ?
                        this.selector1(cur) :
                        this.selector2(cur, this.count);
                }
                return ret;
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            return new SelectEnumerator<TSource, TResult>(source, selector, null);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
            return new SelectEnumerator<TSource, TResult>(source, null, selector);
        }

        #endregion Select

        #region SelectMany

        public static IEnumerable<TResult> SelectMany<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
            foreach (var item in source) {
                foreach (var innerItem in selector(item)) {
                    yield return innerItem;
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) {
            int i = 0;
            foreach (var item in source) {
                foreach (var innerItem in selector(item, i)) {
                    yield return innerItem;
                }
                i++;
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>
            (this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            foreach (var item in source) {
                foreach (var innerItem in collectionSelector(item)) {
                    yield return resultSelector(item, innerItem);
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>
            (this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            int i = 0;
            foreach (var item in source) {
                foreach (var innerItem in collectionSelector(item, i)) {
                    yield return resultSelector(item, innerItem);
                }
                i++;
            }
        }

        #endregion

        #region Skip, SkipWhile

        class SkipEnumerator<T> : IEnumerable<T>, IEnumerator<T> {

            internal SkipEnumerator(IEnumerable<T> source, int count) {
                this.source = source;
                this.count = count;
            }

            private IEnumerable<T> source;
            private int count;
            private IEnumerator<T> en;

            public IEnumerator<T> GetEnumerator() {
                return new SkipEnumerator<T>(this.source, this.count);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new SkipEnumerator<T>(this.source, this.count);
            }

            public T Current {
                get { return this.en.Current; }
            }

            public void Dispose() {
                if (this.en != null) {
                    this.en.Dispose();
                    this.en = null;
                }
            }

            object IEnumerator.Current {
                get { return this.en.Current; }
            }

            public bool MoveNext() {
                if (this.en == null) {
                    this.en = this.source.GetEnumerator();
                }
                do {
                    if (!this.en.MoveNext()) {
                        return false;
                    }
                    this.count--;
                } while (this.count >= 0);
                return true;
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count) {
            return new SkipEnumerator<TSource>(source, count);
            //foreach (var item in source) {
            //    if (--count < 0) {
            //        yield return item;
            //    }
            //}
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            bool skipping = true;
            foreach (var item in source) {
                if (skipping && !predicate(item)) {
                    skipping = false;
                }
                if (!skipping) {
                    yield return item;
                }
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            bool skipping = true;
            int i = 0;
            foreach (var item in source) {
                if (skipping && !predicate(item, i)) {
                    skipping = false;
                }
                i++;
                if (!skipping) {
                    yield return item;
                }
            }
        }

        #endregion

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

        #region Take, TakeWhile

        class TakeEnumerator<T> : IEnumerable<T>, IEnumerator<T> {

            internal TakeEnumerator(IEnumerable<T> source, int count) {
                this.source = source;
                this.count = count;
            }

            private IEnumerable<T> source;
            private int count;
            private IEnumerator<T> en;

            public IEnumerator<T> GetEnumerator() {
                return new TakeEnumerator<T>(this.source, this.count);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new TakeEnumerator<T>(this.source, this.count);
            }

            public T Current {
                get { return this.en.Current; }
            }

            public void Dispose() {
                if (this.en != null) {
                    this.en.Dispose();
                    this.en = null;
                }
            }

            object IEnumerator.Current {
                get { return this.en.Current; }
            }

            public bool MoveNext() {
                count--;
                if (count < 0) {
                    return false;
                }
                if (this.en == null) {
                    this.en = this.source.GetEnumerator();
                }
                return this.en.MoveNext();
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count) {
            return new TakeEnumerator<TSource>(source, count);
            //foreach (var item in source) {
            //    if (count <= 0) {
            //        break;
            //    }
            //    count--;
            //    yield return item;
            //}
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source) {
                if (!predicate(item)) {
                    break;
                }
                yield return item;
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            int i = 0;
            foreach (var item in source) {
                if (!predicate(item, i)) {
                    break;
                }
                i++;
                yield return item;
            }
        }

        #endregion

        #region Union

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            return first.Concat(second).Distinct();
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
            return first.Concat(second).Distinct(comparer);
        }

        #endregion

        #region Where

        class WhereEnumerator<T> : IEnumerable<T>, IEnumerator<T> {

            internal WhereEnumerator(IEnumerable<T> source, Func<T, bool> predicate1, Func<T, int, bool> predicate2) {
                this.source = source;
                this.predicate1 = predicate1;
                this.predicate2 = predicate2;
            }

            private IEnumerable<T> source;
            private Func<T, bool> predicate1;
            private Func<T, int, bool> predicate2;
            private IEnumerator<T> en;
            private int count;
            private T current;

            public IEnumerator<T> GetEnumerator() {
                return new WhereEnumerator<T>(this.source, this.predicate1, this.predicate2);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new WhereEnumerator<T>(this.source, this.predicate1, this.predicate2);
            }

            public T Current {
                get { return this.current; }
            }

            public void Dispose() {
                if (this.en != null) {
                    this.en.Dispose();
                }
            }

            object IEnumerator.Current {
                get { return this.current; }
            }

            public bool MoveNext() {
                if (this.en == null) {
                    this.en = this.source.GetEnumerator();
                    this.count = -1;
                }
                for (; ; ) {
                    if (!this.en.MoveNext()) {
                        return false;
                    }
                    this.count++;
                    this.current = this.en.Current;
                    if ((predicate1 != null) ? (this.predicate1(this.current)) : (this.predicate2(this.current, this.count))) {
                        return true;
                    }
                }
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            return new WhereEnumerator<T>(source, predicate, null);
            //foreach (var item in source) {
            //    if (predicate(item)) {
            //        yield return item;
            //    }
            //}
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, int, bool> predicate) {
            return new WhereEnumerator<T>(source, null, predicate);
            //int i = 0;
            //foreach (var item in source) {
            //    if (predicate(item, i)) {
            //        yield return item;
            //    }
            //    i++;
            //}
        }

        #endregion

        #region Zip

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) {
            IEnumerator<TFirst> enFirst = null;
            IEnumerator<TSecond> enSecond = null;
            try {
                enFirst = first.GetEnumerator();
                enSecond = second.GetEnumerator();
                while (enFirst.MoveNext() && enSecond.MoveNext()) {
                    yield return resultSelector(enFirst.Current, enSecond.Current);
                }
            } finally {
                if (enFirst != null) {
                    enFirst.Dispose();
                }
                if (enSecond != null) {
                    enSecond.Dispose();
                }
            }
        }

        #endregion

        #region ToArray, ToList, ToDictionary, ToLookup

        public static T[] ToArray<T>(this IEnumerable<T> source) {
            return new List<T>(source).ToArray();
        }

        public static List<T> ToList<T>(this IEnumerable<T> source) {
            return new List<T>(source);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            var d = new Dictionary<TKey, TSource>();
            foreach (var item in source) {
                d.Add(keySelector(item), item);
            }
            return d;
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            var d = new Dictionary<TKey, TSource>(comparer);
            foreach (var item in source) {
                d.Add(keySelector(item), item);
            }
            return d;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            var d = new Dictionary<TKey, TElement>();
            foreach (var item in source) {
                d.Add(keySelector(item), elementSelector(item));
            }
            return d;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            var d = new Dictionary<TKey, TElement>(comparer);
            foreach (var item in source) {
                d.Add(keySelector(item), elementSelector(item));
            }
            return d;
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            return source.ToLookup(keySelector, null);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return source.ToLookup(keySelector, x => x, comparer);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) {
            return source.ToLookup(keySelector, elementSelector, null);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            var d = new Dictionary<TKey, List<TElement>>(comparer);
            foreach (var item in source) {
                List<TElement> list;
                var key = keySelector(item);
                var element = elementSelector(item);
                if (d.TryGetValue(key, out list)) {
                    list.Add(element);
                } else {
                    list = new List<TElement> { element };
                    d.Add(key, list);
                }
            }
            return new Lookup<TKey, TElement>(d);
        }

        #endregion

        #region Empty, Range, Repeat, SequenceEqual

        public static IEnumerable<TResult> Empty<TResult>() {
            return new TResult[0];
        }

        class RangeEnumerator : IEnumerable<int>, IEnumerator<int> {

            internal RangeEnumerator(int start, int count) {
                this.value = start - 1;
                this.count = count;
            }

            private int value, count;

            public IEnumerator<int> GetEnumerator() {
                return new RangeEnumerator(this.value + 1, this.count);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new RangeEnumerator(this.value + 1, this.count);
            }

            public int Current {
                get { return this.value; }
            }

            public void Dispose() {
            }

            object IEnumerator.Current {
                get { return this.value; }
            }

            public bool MoveNext() {
                this.value++;
                return --this.count >= 0;
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<int> Range(int start, int count) {
            return new RangeEnumerator(start, count);
            //for (int i = 0; i < count; i++) {
            //    yield return start + i;
            //}
        }

        class RepeatEnumerator<T> : IEnumerable<T>, IEnumerator<T> {

            internal RepeatEnumerator(T element, int count) {
                this.element = element;
                this.count = count;
            }

            private T element;
            private int count;

            public IEnumerator<T> GetEnumerator() {
                return new RepeatEnumerator<T>(this.element, this.count);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new RepeatEnumerator<T>(this.element, this.count);
            }

            public T Current {
                get { return this.element; }
            }

            public void Dispose() {
            }

            object IEnumerator.Current {
                get { return this.element; }
            }

            public bool MoveNext() {
                return --this.count >= 0;
            }

            public void Reset() {
                throw new NotImplementedException();
            }
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) {
            return new RepeatEnumerator<TResult>(element, count);
            //while (--count >= 0) {
            //    yield return element;
            //}
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            return first.SequenceEqual(second, null);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
            if (comparer == null) {
                comparer = EqualityComparer<TSource>.Default;
            }
            IEnumerator<TSource> enFirst = null;
            IEnumerator<TSource> enSecond = null;
            try {
                enFirst = first.GetEnumerator();
                enSecond = second.GetEnumerator();
                for (; ; ) {
                    var validFirst = enFirst.MoveNext();
                    var validSecond = enSecond.MoveNext();
                    if (validFirst != validSecond) {
                        return false;
                    }
                    if (!validFirst) {
                        return true;
                    }
                    if (!comparer.Equals(enFirst.Current, enSecond.Current)) {
                        return false;
                    }
                }
            } finally {
                if (enFirst != null) {
                    enFirst.Dispose();
                }
                if (enSecond != null) {
                    enSecond.Dispose();
                }
            }
        }

        #endregion

    }
}
