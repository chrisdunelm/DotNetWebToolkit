using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    sealed class _StringBuilder {

        public _StringBuilder() {
            this.s = "";
        }

        public _StringBuilder(int capacity) {
            this.s = "";
        }

        public _StringBuilder(string value) {
            this.s = value ?? "";
        }

        public _StringBuilder(int capacity, int maxCapacity) {
            this.s = "";
        }

        public _StringBuilder(string value, int capacity) {
            this.s = value ?? "";
        }

        public _StringBuilder(string value, int startIndex, int length, int capacity) {
            this.s = (value ?? "").Substring(startIndex, length);
        }

        private string s;

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(bool) })]
        public _StringBuilder Append(bool value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(byte) })]
        public _StringBuilder Append(byte value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(char) })]
        public _StringBuilder Append(char value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(char[]) })]
        public _StringBuilder Append(char[] value) {
            this.s += new string(value);
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(decimal) })]
        public _StringBuilder Append(decimal value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(double) })]
        public _StringBuilder Append(double value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(short) })]
        public _StringBuilder Append(short value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(int) })]
        public _StringBuilder Append(int value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(long) })]
        public _StringBuilder Append(long value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(object) })]
        public _StringBuilder Append(object value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(sbyte) })]
        public _StringBuilder Append(sbyte value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(float) })]
        public _StringBuilder Append(float value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(string) })]
        public _StringBuilder Append(string value) {
            this.s += value;
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(ushort) })]
        public _StringBuilder Append(ushort value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(uint) })]
        public _StringBuilder Append(uint value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(ulong) })]
        public _StringBuilder Append(ulong value) {
            this.s += value.ToString();
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(char), typeof(int) })]
        public _StringBuilder Append(char value, int repeatCount) {
            this.s += new string(value, repeatCount);
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(char[]), typeof(int), typeof(int) })]
        public _StringBuilder Append(char[] value, int startIndex, int charCount) {
            this.s += new string(value, startIndex, charCount);
            return this;
        }

        [JsDetail(Signature = new[] { typeof(StringBuilder), typeof(string), typeof(int), typeof(int) })]
        public _StringBuilder Append(string value, int startIndex, int count) {
            this.s += value.Substring(startIndex, count);
            return this;
        }

        public override string ToString() {
            return this.s;
        }

    }
}
