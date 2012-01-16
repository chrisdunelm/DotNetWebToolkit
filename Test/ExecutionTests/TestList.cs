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
        public void TestAddGetIndex() {
            Func<int, int, int, int> f = (a, b, c) => {
                var list = new List<int> { a, b, c };
                return list[0] + list[1] + list[2];
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
        public void TestClear() {
            Func<int> f = () => {
                var list = new List<int> { 1 };
                list.Clear();
                return list.Count;
            };
            this.Test(f);
        }

        [Test, Ignore("This requires the whole default comparer infrastructure to work, which isn't handled yet")]
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

    }

}
