using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Dictionary<TKey, TValue> : IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary, ICollection, IEnumerable {

        class KeyValue {
            public int keyHash;
            public TKey key;
            public TValue value;
        }

        public _Dictionary() : this((IEqualityComparer<TKey>)null) { }

        public _Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public _Dictionary(int capacity) : this((IEqualityComparer<TKey>)null) { }

        public _Dictionary(int capacity, IEqualityComparer<TKey> comparer) : this(comparer) { }

        public _Dictionary(IEqualityComparer<TKey> comparer) {
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            this.buckets = new List<KeyValue>[15];
        }

        public _Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            this.buckets = new List<KeyValue>[15];
        }

        private List<KeyValue>[] buckets;
        private IEqualityComparer<TKey> comparer;
        private int count;

        private void Add(TKey key, TValue value, bool allowOverwrite) {
            var keyHash = this.comparer.GetHashCode(key);
            var bucketIdx = keyHash % this.buckets.Length;
            var items = this.buckets[bucketIdx];
            if (items != null) {
                for (int i = 0; i < items.Count; i++) {
                    var item = items[i];
                    if (this.comparer.GetHashCode(item.key) == keyHash && this.comparer.Equals(item.key, key)) {
                        // Found
                        if (allowOverwrite) {
                            item.value = value;
                            return;
                        } else {
                            throw new ArgumentException();
                        }
                    }
                }
            } else {
                items = new List<KeyValue>();
                this.buckets[bucketIdx] = items;
            }
            items.Add(new KeyValue { keyHash = keyHash, key = key, value = value });
            this.count++;
        }

        public void Add(TKey key, TValue value) {
            this.Add(key, value, false);
        }

        public bool ContainsKey(TKey key) {
            TValue dummy;
            return this.TryGetValue(key, out dummy);
        }

        public ICollection<TKey> Keys {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key) {
            var keyHash = this.comparer.GetHashCode(key);
            var items = this.buckets[keyHash % this.buckets.Length];
            if (items != null) {
                for (int i = 0; i < items.Count; i++) {
                    var item = items[i];
                    if (item.keyHash == keyHash && comparer.Equals(item.key, key)) {
                        items.RemoveAt(i);
                        this.count--;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            var keyHash = this.comparer.GetHashCode(key);
            var items = this.buckets[keyHash % this.buckets.Length];
            if (items != null) {
                for (int i = 0; i < items.Count; i++) {
                    var item = items[i];
                    if (item.keyHash == keyHash && this.comparer.Equals(item.key, key)) {
                        value = item.value;
                        return true;
                    }
                }
            }
            value = default(TValue);
            return false;
        }

        public ICollection<TValue> Values {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key] {
            get {
                TValue result;
                if (this.TryGetValue(key, out result)) {
                    return result;
                }
                throw new KeyNotFoundException();
            }
            set {
                this.Add(key, value, true);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            this.buckets = new List<KeyValue>[15];
            this.count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return this.count; }
        }

        public bool IsReadOnly {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        public void Add(object key, object value) {
            throw new NotImplementedException();
        }

        public bool Contains(object key) {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            throw new NotImplementedException();
        }

        public bool IsFixedSize {
            get { throw new NotImplementedException(); }
        }

        ICollection IDictionary.Keys {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object key) {
            throw new NotImplementedException();
        }

        ICollection IDictionary.Values {
            get { throw new NotImplementedException(); }
        }

        public object this[object key] {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public bool IsSynchronized {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot {
            get { throw new NotImplementedException(); }
        }
    }

}
