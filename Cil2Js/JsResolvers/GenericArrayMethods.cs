using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    class GenericArrayMethods<T> : IList<T>, ICollection<T>, IEnumerable<T> {

        class GenericEnumerator : IEnumerator<T> {
            public GenericEnumerator(T[] array) {
                this.array = array;
                this.index = -1;
            }
            private T[] array;
            private int index;
            public T Current {
                get { return this.array[this.index]; }
            }

            public void Dispose() {
            }

            object IEnumerator.Current {
                get { return this.Current; }
            }

            public bool MoveNext() {
                this.index++;
                return this.index < this.array.Length;
            }

            public void Reset() {
                this.index = -1;
            }
        }

        // TODO: Get rid of all the casts

        public virtual IEnumerator<T> GetEnumerator() {
            return new GenericEnumerator((T[])(object)this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public virtual int Count {
            get {
                return ((T[])(object)this).Length;
            }
        }

        public virtual bool IsReadOnly {
            get {
                return true;
            }
        }

        public virtual bool Contains(T item) {
            return ((IList<T>)this).IndexOf(item) >= 0;
        }


        public virtual int IndexOf(T item) {
            var array = (T[])(object)this;
            if (item == null) {
                for (int i = 0, n = array.Length; i < n; i++) {
                    if (array[i] == null) {
                        return i;
                    }
                }
            } else {
                for (int i = 0, n = array.Length; i < n; i++) {
                    if (item.Equals(array[i])) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public virtual void Insert(int index, T item) {
            throw new NotSupportedException();
        }

        public virtual void RemoveAt(int index) {
            throw new NotSupportedException();
        }

        public virtual T this[int index] {
            get {
                return ((T[])(object)this)[index];
            }
            set {
                ((T[])(object)this)[index] = value;
            }
        }

        public virtual void Add(T item) {
            throw new NotSupportedException();
        }

        public virtual void Clear() {
            throw new NotSupportedException();
        }

        public virtual void CopyTo(T[] array, int arrayIndex) {
            throw new NotSupportedException();
        }

        public virtual bool Remove(T item) {
            throw new NotSupportedException();
        }

    }
}
