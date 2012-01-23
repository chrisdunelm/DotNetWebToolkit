﻿using System;
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

        public StringBuilder Append(char[] value) {
            this.s += new string(value);
            return this;
        }

        public StringBuilder Append(decimal value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(double value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(short value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(int value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(long value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(object value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(sbyte value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(float value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(string value) {
            this.s += value;
            return this;
        }

        public StringBuilder Append(ushort value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(uint value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(ulong value) {
            this.s += value.ToString();
            return this;
        }

        public StringBuilder Append(char value, int repeatCount) {
            this.s += new string(value, repeatCount);
            return this;
        }

        public StringBuilder Append(char[] value, int startIndex, int charCount) {
            this.s += new string(value, startIndex, charCount);
            return this;
        }

        public StringBuilder Append(string value, int startIndex, int count) {
            this.s += value.Substring(startIndex, count);
            return this;
        }

        public override string ToString() {
            return this.s;
        }

    }
}
