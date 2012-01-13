using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Utils {
    class GenericArrayMethods<T> {

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

        public virtual IEnumerator<T> GetEnumerator() {
            return new GenericEnumerator((T[])(object)this);
        }

    }
}
