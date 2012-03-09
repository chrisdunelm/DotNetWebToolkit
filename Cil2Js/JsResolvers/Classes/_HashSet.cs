using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _HashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable {

        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator {

            internal Enumerator(_HashSet<T> hashSet) {
                this.en = hashSet.d.GetEnumerator();
            }

            private Dictionary<T, object>.Enumerator en;

            public T Current {
                get { return this.en.Current.Key; }
            }

            public void Dispose() {
            }

            object IEnumerator.Current {
                get { return this.en.Current.Key; }
            }

            public bool MoveNext() {
                return this.en.MoveNext();
            }

            void IEnumerator.Reset() {
                ((IEnumerator)this.en).Reset();
            }
        }

        public _HashSet() : this(null, null) { }

        public _HashSet(IEqualityComparer<T> comparer) : this(null, comparer) { }

        public _HashSet(IEnumerable<T> collection) : this(collection, null) { }

        public _HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) {
            this.d = new Dictionary<T, object>(comparer);
            if (collection != null) {
                foreach (var item in collection) {
                    this.d[item] = null;
                }
            }
        }

        private Dictionary<T, object> d;

        public bool Add(T item) {
            if (d.ContainsKey(item)) {
                return false;
            } else {
                d.Add(item, null);
                return true;
            }
        }

        public void ExceptWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item) {
            this.Add(item);
        }

        public void Clear() {
            this.d.Clear();
        }

        public bool Contains(T item) {
            return this.d.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return this.d.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(T item) {
            return this.d.Remove(item);
        }

        [JsDetail(Signature = new[] { typeof(HashSet<GenTypeParam0>.Enumerator) })]
        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }
    }
}
