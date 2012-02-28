using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestLinq : ExecutionTestBase {

        [Test]
        public void TestWhere() {
            Func<int, int, int, int> f = (a, b, c) => {
                var array = new[] { a, b };
                var q = array.Where(x => x > c);
                return q.Count();
            };
            this.Test(f);
        }

        [Test]
        public void TestSelect() {
            Func<int, int, int, double> f = (a, b, c) => {
                var array = new[] { a, b, c };
                return array.Select(x => (double)x / 2).Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestToArray() {
            Func<int, int, int> f = (a, b) => {
                var e = (IEnumerable<int>)new List<int> { a, b };
                var array = e.ToArray();
                return array[0] + array[1] * 100;
            };
            this.Test(f);
        }

        [Test]
        public void TestSumInt32() {
            Func<int, int, int, int> f = (a, b, c) => {
                var array = new[] { a, b, c };
                return array.Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestSumDouble() {
            Func<double, double, double, double> f = (a, b, c) => {
                var array = new[] { a, b, c };
                return array.Sum();
            };
            this.Test(f);
        }

    }

}
