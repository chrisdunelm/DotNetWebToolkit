using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestList : ExecutionTestBase {

        [Test]
        public void TestCtorIEnumerable() {
            Func<int, int, bool> f = (a, b) => {
                var list = new List<int>(new[] { a, b });
                return list[0] == a && list[1] == b;
            };
            this.Test(f);
        }

        [Test]
        public void TestAddGetIndex() {
            Func<int, int, int, int> f = (a, b, c) => {
                var list = new List<int> { a, b, c };
                return list[0] + list[1] * 100 + list[2] * 10000;
            };
            this.Test(f);
        }

        [Test]
        public void TestSetGetIndex() {
            Func<int, int, int> f = (a, b) => {
                var list = new List<int> { 0, 0 };
                list[0] = a;
                list[1] = b;
                return list[0] + list[1] * 100;
            };
            this.Test(f);
        }

        [Test]
        public void TestManyAdds() {
            Func<int, int> f = a => {
                var list = new List<int>();
                for (int i = 0; i < a; i++) {
                    list.Add(i);
                }
                int r = 0;
                for (int i = 0; i < list.Count; i++) {
                    r += list[i];
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void TestAddRange() {
            Func<int, int, int, bool> f = (a, b, c) => {
                var list = new List<int>();
                list.AddRange(new[] { a, b, c });
                return list[0] == a && list[1] == b && list[2] == c;
            };
            this.Test(f);
        }

        [Test]
        public void TestInsert() {
            Func<int, int, int, int> f = (a, b, c) => {
                var list = new List<int> { 1, 2, 3 };
                list.Insert(3, a);
                list.Insert(1, b);
                list.Insert(0, c);
                return list[0] + list[1] * 10 + list[2] * 100 + list[3] * 1000 + list[4] * 10000 + list[5] * 100000;
            };
            this.Test(f);
        }

        [Test]
        public void TestInsertRange() {
            Func<int, int, bool> f = (a, b) => {
                var list = new List<int> { 1, 2 };
                list.InsertRange(1, new[] { a, b });
                return list[0] == 1 && list[1] == a && list[2] == b && list[3] == 2;
            };
            this.Test(f);
        }

        [Test]
        public void TestClear() {
            Func<int> f = () => {
                var list = new List<int> { 1 };
                list.Clear();
                return list.Count;
            };
            this.Test(f);
        }

        [Test]
        public void TestIndexOf() {
            Func<int> f1ArgNone = () => {
                var list = new List<int> { 1, 2, 3, 4 };
                return list.IndexOf(0);
            };
            Func<int> f1ArgMatch1 = () => {
                var list = new List<int> { 1, 2, 3, 4 };
                return list.IndexOf(1);
            };
            Func<int> f1ArgMatch2 = () => {
                var list = new List<int> { 1, 2, 3, 4 };
                return list.IndexOf(4);
            };
            this.Test(f1ArgNone, f1ArgMatch1, f1ArgMatch2);
        }

        [Test]
        public void TestSort() {
            Func<int, int, int, int, bool> f = (a, b, c, d) => {
                var list = new List<int> { a, b, c, d };
                list.Sort();
                return list[0] <= list[1] && list[1] <= list[2] && list[2] <= list[3];
            };
            this.Test(f);
        }

        class IntNegComp : IComparer<int> {
            public int Compare(int x, int y) {
                return y - x;
            }
        }
        [Test]
        public void TestSortComparer() {
            Func<int, int, int, bool> f = (a, b, c) => {
                var list = new List<int> { a, b, c };
                list.Sort(new IntNegComp());
                return list[0] >= list[1] && list[1] >= list[2];
            };
            this.Test(f);
        }

        [Test]
        public void TestBinarySearch() {
            Func<int, int, int, int> f = (a, b, c) => {
                var list = new List<int> { a, b, c };
                list.Sort();
                var aIdx = list.BinarySearch(a);
                var bIdx = list.BinarySearch(b);
                var cIdx = list.BinarySearch(c);
                var yIdx = ~list.BinarySearch(-1);
                var zIdx = ~list.BinarySearch(10001);
                var midIdx = ~list.BinarySearch(a - 1);
                return aIdx + bIdx * 10 + cIdx * 100 + yIdx * 1000 + zIdx * 10000 + midIdx * 100000;
            };
            this.Test(f);
        }

        [Test]
        public void TestBinarySearchComparer() {
            Func<int, int, int, int> f = (a, b, c) => {
                var list = new List<int> { a, b, c };
                var comp = new IntNegComp();
                list.Sort(comp);
                var aIdx = list.BinarySearch(a, comp);
                var bIdx = list.BinarySearch(b, comp);
                var cIdx = list.BinarySearch(c, comp);
                var yIdx = ~list.BinarySearch(-1, comp);
                var zIdx = ~list.BinarySearch(10001, comp);
                var midIdx = ~list.BinarySearch(a - 1, comp);
                return aIdx + bIdx * 10 + cIdx * 100 + yIdx * 1000 + zIdx * 10000 + midIdx * 100000;
            };
            this.Test(f);
        }

    }

}
