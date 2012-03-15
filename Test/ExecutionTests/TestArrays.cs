using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestArrays : ExecutionTestBase {

        [Test]
        public void TestCreateSetGet1() {
            Func<int, int> f = a => {
                var array = new int[1];
                array[0] = a;
                return array[0];
            };
            this.Test(f);
        }

        [Test]
        public void TestCreateSetGetN() {
            Func<int, int, int> f = (a, b) => {
                var array = new int[a];
                for (int i = 0; i < a; i++) {
                    array[i] = b + i;
                }
                int r = 0;
                for (int i = 0; i < a; i++) {
                    r += array[i];
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void TestLength() {
            Func<int, int> f = a => {
                var array = new int[a];
                return array.Length;
            };
            this.Test(f);
        }

        [Test]
        public void TestIndexOfT() {
            Func<int, int> f = a => {
                var array = new int[] { 1, 2, 3, 4, 5, 6 };
                return Array.IndexOf(array, a & 7);
            };
            this.Test(f);
        }

        [Test]
        public void TestIndexOfArray() {
            Func<int, int> f = a => {
                var array = (Array)new int[] { 1, 2, 3, 4, 5, 6 };
                return Array.IndexOf(array, a);
            };
            this.Test(f);
        }

        [Test]
        public void TestEnumerableT() {
            Func<int, int, int, int> f = (a, b, c) => {
                IEnumerable<int> array = new[] { a, b, c };
                var ret = 0;
                foreach (var i in array) {
                    ret += i;
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestEnumerable() {
            Func<int, int, int, int> f = (a, b, c) => {
                IEnumerable array = new[] { a, b, c };
                int ret = 0;
                foreach (int i in array) {
                    ret += i;
                }
                return ret;
            };
            this.Test(f);
        }

        class CTestIEnumerableWithTwoTypes {
            public int Value;
        }

        [Test]
        public void TestIEnumerableTWithTwoTypes() {
            Func<int, int, int, int, int, int, int> f = (a, b, c, x, y, z) => {
                IEnumerable<int> array1 = new[] { a, b, c };
                IEnumerable<CTestIEnumerableWithTwoTypes> array2 = new[]{
                    new CTestIEnumerableWithTwoTypes { Value = x },
                    new CTestIEnumerableWithTwoTypes { Value = y },
                    new CTestIEnumerableWithTwoTypes { Value = z } };
                var ret = 0;
                foreach (var i in array1) {
                    ret += i;
                }
                foreach (var v in array2) {
                    ret += v.Value;
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestICollectionT() {
            Func<int, int, int, int> f = (a, b, c) => {
                ICollection<int> collection = new[] { a, b, c };
                var ret = 0;
                ret += collection.Count;
                ret += collection.IsReadOnly ? 4 : -4;
                ret += collection.Contains(a) ? 5 : -5;
                ret += collection.Contains(b) ? 6 : -6;
                ret += collection.Contains(c) ? 7 : -7;
                ret += collection.Contains(0) ? 8 : -8;
                ret += collection.Contains(1) ? 9 : -9;
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestIListT() {
            Func<int, int, int, int> f = (a, b, c) => {
                IList<int> list = new[] { a, b, c };
                int ret = 0;
                for (int i = 0; i < 3; i++) {
                    ret += list[i];
                    list[i]++;
                    ret -= list[i];
                }
                ret += list.IndexOf(a);
                ret += list.IndexOf(b);
                ret += list.IndexOf(c);
                ret += list.IndexOf(-1);
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayCopy() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var array1 = new[] { a, b, c, d };
                var array2 = new int[2];
                Array.Copy(array1, 1, array2, 0, 2);
                return array2[0] + array2[1];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationString() {
            Func<string> f = () => {
                var a = new string[] { "a", "b", "c", "d", "e", "f" };
                return a[0] + a[1] + a[2] + a[3] + a[4] + a[5];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationByte() {
            Func<int> f = () => {
                var a = new byte[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationSByte() {
            Func<int> f = () => {
                var a = new sbyte[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationInt16() {
            Func<int> f = () => {
                var a = new short[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationInt32() {
            Func<int> f = () => {
                var a = new int[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationInt64() {
            Func<long> f = () => {
                var a = new long[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationUInt16() {
            Func<int> f = () => {
                var a = new ushort[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationUInt32() {
            Func<uint> f = () => {
                var a = new uint[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationUInt64() {
            Func<ulong> f = () => {
                var a = new ulong[] { 1, 2, 3, 4, 5 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationSingle() {
            Func<float> f = () => {
                var a = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 5.0f };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationDouble() {
            Func<double> f = () => {
                var a = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
                return a[0] + a[1] + a[2] + a[3] + a[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestArrayInitialisationBool() {
            Func<string> f = () => {
                var a = new bool[] { true, true, false, true, false, false, false, true };
                return a[0].ToString() + a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7];
            };
            this.Test(f);
        }

        private static object ForceObject(object o) {
            return o;
        }

        [Test]
        public void TestCastArrayAccess() {
            Func<int, int> f = a => {
                var array = new int[1];
                array[0] = a;
                object o = ForceObject(array);
                var array2 = (int[])o;
                return array2[0];
            };
            this.Test(f);
        }

        [Test]
        public void TestSort() {
            Func<int, int, int, int, int, bool> f = (a, b, c, d, e) => {
                var array = new int[] { a, b, c, d, e };
                Array.Sort(array);
                return array[0] <= array[1] && array[1] <= array[2] && array[2] <= array[3] && array[3] <= array[4];
            };
            this.Test(f);
        }

        [Test]
        public void TestSortWithRange() {
            Func<int, int, int, int, int, bool> f = (a, b, c, d, e) => {
                var array = new int[] { a, b, c, d, e };
                Array.Sort(array, 1, 3);
                return array[0] == a && array[4] == e && array[1] <= array[2] && array[2] <= array[3];
            };
            this.Test(f);
        }

        [Test]
        public void TestBinarySearch() {
            Func<int, int, int, int, int, int> f = (a, b, c, d, e) => {
                var array = new int[] { a, b, c, d, e };
                Array.Sort(array);
                for (int i = 0; i < 4; i++) {
                    if (array[i] == array[i + 1]) {
                        array[i] = 200 + i;
                    }
                }
                Array.Sort(array);
                var aIdx = Array.BinarySearch(array, a);
                var bIdx = Array.BinarySearch(array, b);
                var cIdx = Array.BinarySearch(array, c);
                var dIdx = Array.BinarySearch(array, d);
                var eIdx = Array.BinarySearch(array, e);
                var yIdx = ~Array.BinarySearch(array, -1);
                var zIdx = ~Array.BinarySearch(array, 10001);
                return aIdx + bIdx * 10 + cIdx * 100 + dIdx * 1000 + eIdx * 10000 + yIdx * 100000 + zIdx * 1000000;
            };
            this.Test(f);
        }

    }
}
