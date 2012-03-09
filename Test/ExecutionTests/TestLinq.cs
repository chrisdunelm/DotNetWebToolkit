using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestLinq : ExecutionTestBase {

        #region Count

        [Test]
        public void TestCountCollection() {
            Func<int, int> f = a => {
                var array = new int[a];
                return array.Count();
            };
            this.Test(f);
        }

        [Test]
        public void TestCountNonCollection() {
            Func<int, int> f = a => {
                var array = new int[a];
                return array.Where(x => x >= 0).Count();
            };
            this.Test(f);
        }

        [Test]
        public void TestCountPredicate() {
            Func<int, int> f = a => {
                var array = new int[a];
                return array.Count(x => x >= 50);
            };
            this.Test(f);
        }

        #endregion

        #region Distinct

        [Test]
        public void TestDistinctUniques() {
            Func<int> f = () => {
                var a = new int[] { 1, 2, 3, 4, 5 };
                return a.Distinct().Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestDistinctOverlaps() {
            Func<int> f = () => {
                var a = new int[] { 1, 2, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 10 };
                return a.Distinct().Sum();
            };
            this.Test(f);
        }

        #endregion

        #region First

        [Test]
        public void TestFirstOk() {
            Func<int, int> f = a => {
                var array = new int[] { a };
                return array.First();
            };
            this.Test(f);
        }

        [Test]
        public void TestFirstEmpty() {
            Func<int> f = () => {
                var array = new int[0];
                try {
                    return array.First();
                } catch {
                    return -1;
                }
            };
            this.Test(f);
        }

        [Test]
        public void TestFirstOrDefaultOk() {
            Func<int, int> f = a => {
                var array = new int[] { a };
                return array.FirstOrDefault();
            };
            this.Test(f);
        }

        [Test]
        public void TestFirstOrDefaultEmpty() {
            Func<int, int> f = a => {
                var array = new int[0];
                return array.FirstOrDefault();
            };
            this.Test(f);
        }

        #endregion

        #region Select

        [Test]
        public void TestSelect() {
            Func<int, int, int, double> f = (a, b, c) => {
                var array = new[] { a, b, c };
                return array.Select(x => (double)x / 2).Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestSelectIndex() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var array = new[] { a, b, c, d };
                return array.Select((x, i) => x * i).Sum();
            };
            this.Test(f);
        }

        #endregion

        #region SelectMany

        [Test]
        public void TestSelectMany() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var a1 = new int[] { a, b };
                var a2 = new int[] { c, d };
                var array = new[] { a1, a2 };
                return array.SelectMany(x => x).Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestSelectManyIndex() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var a1 = new int[] { a, b };
                var a2 = new int[] { c, d };
                var array = new[] { a1, a2 };
                return array.SelectMany((x, i) => x.Select(y => y * (i + 1))).Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestSelectManyCollection() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var a1 = new int[] { a, b };
                var a2 = new int[] { c, d };
                var array = new[] { a1, a2 };
                return array.SelectMany(x => x, (s, x) => s[0] * x).Sum();
            };
            this.Test(f);
        }

        [Test]
        public void TestSelectManyIndexCollection() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var a1 = new int[] { a, b };
                var a2 = new int[] { c, d };
                var array = new[] { a1, a2 };
                return array.SelectMany((x, i) => x.Select(y => y * (i + 1)), (s, x) => s[0] * x).Sum();
            };
            this.Test(f);
        }

        #endregion

        #region Sum

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

        #endregion

        #region Where

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
        public void TestWhereIndex() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                var array = new[] { a, b, c, d };
                var q = array.Where((x, i) => x > 50 && i >= 2);
                return q.Sum();
            };
            this.Test(f);
        }

        #endregion

        #region Converters

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
        public void TestToList() {
            Func<int, int, int> f = (a, b) => {
                var e = (IEnumerable<int>)new List<int> { a, b };
                var list = e.ToList();
                return list[0] + list[1] * 100;
            };
            this.Test(f);
        }

        [Test]
        public void TestToDictionary() {
            Func<int, int> f = a => {
                var array = new[] { a };
                var d = array.ToDictionary(x => x);
                return d[a];
            };
            this.Test(f);
        }

        [Test]
        public void TestToDictionaryComparer() {
            Func<int, int> f = a => {
                var array = new[] { a };
                var d = array.ToDictionary(x => x, EqualityComparer<int>.Default);
                return d[a];
            };
            this.Test(f);
        }

        [Test]
        public void TestToDictionaryElementSelector() {
            Func<int, string> f = a => {
                var array = new[] { a };
                var d = array.ToDictionary(x => x, x => x.ToString());
                return d[a];
            };
            this.Test(f);
        }

        [Test]
        public void TestToDictionaryElementSelectorComparer() {
            Func<int, string> f = a => {
                var array = new[] { a };
                var d = array.ToDictionary(x => x, x => x.ToString(), EqualityComparer<int>.Default);
                return d[a];
            };
            this.Test(f);
        }

        #endregion

    }

}
