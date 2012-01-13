using System;
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
        public void TestEnumerable() {
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

        class CTestIEnumerableWithTwoTypes {
            public int Value;
        }

        [Test]
        public void TestIEnumerableWithTwoTypes() {
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
        public void TestICollection() {
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
        public void TestIList() {
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

    }
}
