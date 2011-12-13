using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Output {
    class NameGenerator {

        private readonly static char[] c = "abcdefghijklmnopqrstuvwxyz".ToArray();

        public NameGenerator(string prefix = "") {
            this.prefix = prefix;
        }

        private string prefix;
        private int curName = 0;

        public string GetNewName() {
            var l = c.Length;
            int length = 1, sub = 0;
            for (int i = l, add = l; ; sub += add, add *= l, i += add, length++) {
                if (this.curName < i) {
                    break;
                }
            }
            var v = this.curName - sub;
            string name = "";
            for (int i = 0; i < length; i++) {
                name += c[v % l];
                v = v / l;
            }
            this.curName++;
            name = this.prefix + new string(name.Reverse().ToArray());
            return name;
        }


    }
}
