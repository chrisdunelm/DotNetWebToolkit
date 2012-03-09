using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary, ICollection, IEnumerable {

        #region Enumerator, key/value collections

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator {

            internal Enumerator(_Dictionary<TKey, TValue> dictionary) {
                this.dictionary = dictionary;
                this.slot = 0;
                this.current = default(KeyValuePair<TKey, TValue>);
            }

            private _Dictionary<TKey, TValue> dictionary;
            private int slot;
            private KeyValuePair<TKey, TValue> current;

            public KeyValuePair<TKey, TValue> Current {
                get { return this.current; }
            }

            public void Dispose() {
            }

            object IEnumerator.Current {
                get { return this.current; }
            }

            public bool MoveNext() {
                while (this.slot < this.dictionary.emptyOfs) {
                    var slot = this.dictionary.slots[this.slot];
                    this.slot++;
                    if (slot != null && slot.hashCode >= 0) {
                        this.current = new KeyValuePair<TKey, TValue>(slot.key, slot.value);
                        return true;
                    }
                }
                return false;
            }

            void IEnumerator.Reset() {
                this.slot = 0;
            }

            DictionaryEntry IDictionaryEnumerator.Entry {
                get { return new DictionaryEntry(this.current.Key, this.current.Value); }
            }

            object IDictionaryEnumerator.Key {
                get { return this.current.Key; }
            }

            object IDictionaryEnumerator.Value {
                get { return this.current.Value; }
            }
        }

        public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable {

            public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator {

                internal Enumerator(_Dictionary<TKey, TValue> dictionary) {
                    this.en = dictionary.GetEnumerator();
                }

                private _Dictionary<TKey, TValue>.Enumerator en;

                public TKey Current {
                    get { return this.en.Current.Key; }
                }

                public void Dispose() {
                }

                object IEnumerator.Current {
                    get { return this.Current; }
                }

                public bool MoveNext() {
                    return this.en.MoveNext();
                }

                void IEnumerator.Reset() {
                    ((IEnumerator)this.en).Reset();
                }
            }

            internal KeyCollection(_Dictionary<TKey, TValue> dictionary) {
                this.dictionary = dictionary;
            }

            private _Dictionary<TKey, TValue> dictionary;

            void ICollection<TKey>.Add(TKey item) {
                throw new NotSupportedException();
            }

            void ICollection<TKey>.Clear() {
                throw new NotSupportedException();
            }

            bool ICollection<TKey>.Contains(TKey item) {
                return this.dictionary.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex) {
                throw new NotImplementedException();
            }

            public int Count {
                get { return this.dictionary.count; }
            }

            bool ICollection<TKey>.IsReadOnly {
                get { return true; }
            }

            bool ICollection<TKey>.Remove(TKey item) {
                throw new NotSupportedException();
            }

            [JsDetail(Signature = new[] { typeof(Dictionary<GenTypeParam0, GenTypeParam1>.KeyCollection.Enumerator) })]
            public Enumerator GetEnumerator() {
                return new Enumerator(this.dictionary);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() {
                return new Enumerator(this.dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new Enumerator(this.dictionary);
            }

            void ICollection.CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            object ICollection.SyncRoot {
                get { return this.dictionary; }
            }
        }

        public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable {

            public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator {

                internal Enumerator(_Dictionary<TKey, TValue> dictionary) {
                    this.en = dictionary.GetEnumerator();
                }

                private _Dictionary<TKey, TValue>.Enumerator en;

                public TValue Current {
                    get { return this.en.Current.Value; }
                }

                public void Dispose() {
                }

                object IEnumerator.Current {
                    get { return this.Current; }
                }

                public bool MoveNext() {
                    return this.en.MoveNext();
                }

                void IEnumerator.Reset() {
                    ((IEnumerator)this.en).Reset();
                }
            }

            internal ValueCollection(_Dictionary<TKey, TValue> dictionary) {
                this.dictionary = dictionary;
            }

            private _Dictionary<TKey, TValue> dictionary;

            void ICollection<TValue>.Add(TValue item) {
                throw new NotSupportedException();
            }

            void ICollection<TValue>.Clear() {
                throw new NotSupportedException();
            }

            bool ICollection<TValue>.Contains(TValue item) {
                return this.dictionary.ContainsValue(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex) {
                throw new NotImplementedException();
            }

            public int Count {
                get { return this.dictionary.count; }
            }

            bool ICollection<TValue>.IsReadOnly {
                get { return true; }
            }

            bool ICollection<TValue>.Remove(TValue item) {
                throw new NotSupportedException();
            }

            [JsDetail(Signature = new[] { typeof(Dictionary<GenTypeParam0, GenTypeParam1>.ValueCollection.Enumerator) })]
            public Enumerator GetEnumerator() {
                return new Enumerator(this.dictionary);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() {
                return new Enumerator(this.dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new Enumerator(this.dictionary);
            }

            void ICollection.CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            object ICollection.SyncRoot {
                get { return this.dictionary; }
            }
        }

        #endregion

        class Slot {
            public int hashCode;
            public int next;
            public TKey key;
            public TValue value;
        }

        public _Dictionary() : this(null, null) { }

        public _Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public _Dictionary(int capacity) : this(null, null) { }

        public _Dictionary(int capacity, IEqualityComparer<TKey> comparer) : this(null, comparer) { }

        public _Dictionary(IEqualityComparer<TKey> comparer) : this(null, comparer) { }

        public _Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            this.Clear();
        }

        private IEqualityComparer<TKey> comparer;
        private int[] buckets;
        private Slot[] slots;
        private int emptyOfs;
        private int freeList;
        private int count;

        private KeyCollection keys = null;
        private ValueCollection values = null;


        private int InternalGetHashCode(TKey key) {
            if (key == null) {
                return 0;
            } else {
                return key.GetHashCode() & 0x7fffffff;
            }
        }

        private void Resize() {
            var newSize = this.emptyOfs * 2 + 1;
            var newBuckets = new int[newSize];
            var newSlots = new Slot[newSize];
            Array.Copy(this.slots, 0, newSlots, 0, this.emptyOfs);
            for (int i = 0; i < this.emptyOfs; i++) {
                var bucket = newSlots[i].hashCode % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            this.buckets = newBuckets;
            this.slots = newSlots;
        }

        private int FindSlot(TKey key) {
            int hashCode = this.InternalGetHashCode(key);
            for (int slot = this.buckets[hashCode % this.buckets.Length] - 1; slot >= 0; slot = this.slots[slot].next) {
                if (this.slots[slot].hashCode == hashCode && this.comparer.Equals(this.slots[slot].key, key)) {
                    return slot;
                }
            }
            return -1;
        }

        private void Add(TKey key, TValue value, bool allowOverwrite) {
            var slot = this.FindSlot(key);
            if (slot >= 0) {
                if (allowOverwrite) {
                    this.slots[slot].value = value;
                    return;
                } else {
                    throw new ArgumentException();
                }
            }
            if (this.freeList >= 0) {
                slot = this.freeList;
                this.freeList = this.slots[slot].next;
            } else {
                if (this.emptyOfs == this.slots.Length) {
                    this.Resize();
                }
                slot = this.emptyOfs;
                this.emptyOfs++;
            }
            var hashCode = this.InternalGetHashCode(key);
            var bucket = hashCode % this.buckets.Length;
            this.slots[slot] = new Slot {
                hashCode = hashCode,
                next = this.buckets[bucket] - 1,
                key = key,
                value = value
            };
            this.buckets[bucket] = slot + 1;
            this.count++;
        }

        public void Add(TKey key, TValue value) {
            this.Add(key, value, false);
        }

        public bool ContainsKey(TKey key) {
            return this.FindSlot(key) >= 0;
        }

        public bool ContainsValue(TValue value) {
            if (value == null) {
                foreach (var slot in this.slots) {
                    if (slot != null && slot.hashCode >= 0 && slot.value == null) {
                        return true;
                    }
                }
            } else {
                var comparer = EqualityComparer<TValue>.Default;
                foreach (var slot in this.slots) {
                    if (slot != null && slot.hashCode >= 0 && comparer.Equals(slot.value, value)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public KeyCollection Keys {
            [JsDetail(Signature = new[] { typeof(Dictionary<GenTypeParam0, GenTypeParam1>.KeyCollection) })]
            get {
                if (this.keys == null) {
                    this.keys = new KeyCollection(this);
                }
                return this.keys;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys {
            get { return this.Keys; }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys {
            get { return this.Keys; }
        }

        public bool Remove(TKey key) {
            int hashCode = this.InternalGetHashCode(key);
            int bucket = hashCode % this.buckets.Length;
            int prevSlot = -1;
            for (int slot = this.buckets[bucket] - 1; slot >= 0; slot = this.slots[slot].next) {
                if (this.slots[slot].hashCode == hashCode && this.comparer.Equals(this.slots[slot].key, key)) {
                    if (prevSlot >= 0) {
                        this.slots[prevSlot].next = this.slots[slot].next;
                    } else {
                        this.buckets[bucket] = this.slots[slot].next + 1;
                    }
                    this.slots[slot].hashCode = -1;
                    this.slots[slot].key = default(TKey);
                    this.slots[slot].value = default(TValue);
                    this.slots[slot].next = this.freeList;
                    this.freeList = slot;
                    return true;
                }
                prevSlot = slot;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            int slot = this.FindSlot(key);
            if (slot >= 0) {
                value = this.slots[slot].value;
                return true;
            } else {
                value = default(TValue);
                return false;
            }
        }

        public ValueCollection Values {
            [JsDetail(Signature = new[] { typeof(Dictionary<GenTypeParam0, GenTypeParam1>.ValueCollection) })]
            get {
                if (this.values == null) {
                    this.values = new ValueCollection(this);
                }
                return this.values;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values {
            get { return this.Values; }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
            get { return this.Values; }
        }

        public TValue this[TKey key] {
            get {
                int slot = this.FindSlot(key);
                if (slot >= 0) {
                    return this.slots[slot].value;
                } else {
                    throw new KeyNotFoundException();
                }
            }
            set {
                this.Add(key, value, true);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            this.Add(item.Key, item.Value, false);
        }

        public void Clear() {
            this.buckets = new int[7];
            this.slots = new Slot[7];
            this.emptyOfs = 0;
            this.freeList = -1;
            this.count = 0;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return this.count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
            get { return false; }
        }

        bool IDictionary.IsReadOnly {
            get { return false; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        [JsDetail(Signature = new[] { typeof(Dictionary<GenTypeParam0, GenTypeParam1>.Enumerator) })]
        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        void IDictionary.Add(object key, object value) {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object key) {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new Enumerator(this);
        }

        bool IDictionary.IsFixedSize {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { throw new NotImplementedException(); }
        }

        void IDictionary.Remove(object key) {
            throw new NotImplementedException();
        }

        ICollection IDictionary.Values {
            get { throw new NotImplementedException(); }
        }

        object IDictionary.this[object key] {
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

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot {
            get { return this; }
        }
    }

}
