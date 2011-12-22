using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Output {
    public class NameGenerator {

        private readonly static char[] c0 = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
        private readonly static char[] cN = c0.Concat("0123456789".ToArray()).ToArray();

        public NameGenerator(string prefix = "") {
            this.prefix = prefix;
            this.gen = this.GenFn().GetEnumerator();
        }

        private string prefix;
        private IEnumerator<string> gen;

        private IEnumerable<string> GenFn() {
            int[] indexes = { 0 };
            for (; ; ) {
                var name = new string(indexes.Select((index, position) => (position == 0 ? c0 : cN)[index]).ToArray());
                yield return this.prefix + name;
                bool ok = false;
                for (int i = indexes.Length - 1; i >= 0; i--) {
                    if (++indexes[i] >= (i == 0 ? c0 : cN).Length) {
                        indexes[i] = 0;
                    } else {
                        ok = true;
                        break;
                    }
                }
                if (!ok) {
                    // Need to start a longer name
                    indexes = new int[indexes.Length + 1]; // All default to 0 which is correct
                }
            }
        }

        public string GetNewName() {
            this.gen.MoveNext();
            return this.gen.Current;
        }


    }
}
