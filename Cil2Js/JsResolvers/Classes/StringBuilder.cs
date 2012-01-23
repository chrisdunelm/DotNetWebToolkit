using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    sealed class StringBuilder {

        public StringBuilder() {
            this.s = "";
        }

        public StringBuilder(int capacity) {
            this.s = "";
        }

        public StringBuilder(string value) {
            this.s = value ?? "";
        }

        public StringBuilder(int capacity, int maxCapacity) {
            this.s = "";
        }

        public StringBuilder(string value, int capacity) {
            this.s = value ?? "";
        }

        public StringBuilder(string value, int startIndex, int length, int capacity) {
            this.s = (value ?? "").Substring(startIndex, length);
        }

        private string s;

        public StringBuilder Append(bool value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(byte value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(char value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(string value) {
            this.s += value;
            return this;
        }

    }
}
