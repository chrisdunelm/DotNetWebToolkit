using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestLinq : ExecutionTestBase {

        #region Concat

        [Test]
        public void TestConcat() {
            Func<bool> f = () => {
                var a1 = new int[] { 0, 1 };
                var a2 = new int[] { 2, 3 };
                var r = a1.Concat(a2).ToArray();
                return r[0] == 0 && r[1] == 1 && r[2] == 2 && r[3] == 3;
            };
            this.Test(f, true);
        }

        #endregion

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

        #region OrderBy, OrderByDescending, ThenBy, ThenByDescending

        [Test]
        public void TestOrderBy() {
            Func<bool> f = () => {
                var a = new int[] { 3, 1, 2 };
                var o = a.OrderBy(x => x).ToArray();
                return o[0] == 1 && o[1] == 2 && o[2] == 3;
            };
            this.Test(f, true);
        }

        [Test]
        public void TestOrderByDescending() {
            Func<bool> f = () => {
                var a = new int[] { 3, 1, 2 };
                var o = a.OrderByDescending(x => x).ToArray();
                return o[0] == 3 && o[1] == 2 && o[2] == 1;
            };
            this.Test(f, true);
        }

        [Test]
        public void TestThenBy() {
            Func<bool> f = () => {
                var a = new int[] { 6, 5, 4, 3, 2, 1 };
                var o = a.OrderBy(x => x & 1).ThenBy(x => x).ToArray();
                return o[0] == 2 && o[1] == 4 && o[2] == 6 && o[3] == 1 && o[4] == 3 && o[5] == 5;
            };
            this.Test(f, true);
        }

        [Test]
        public void TestThenByDescending() {
            Func<bool> f = () => {
                var a = new int[] { 6, 5, 4, 3, 2, 1 };
                var o = a.OrderBy(x => x & 1).ThenByDescending(x => x).ToArray();
                return o[0] == 6 && o[1] == 4 && o[2] == 2 && o[3] == 5 && o[4] == 3 && o[5] == 1;
            };
            this.Test(f, true);
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

        #region Skip, SkipWhile

        public void TestSkip() {
            Func<int, int> f = a => Enumerable.Range(0, 50).Skip(a).Sum();
            this.Test(f);
        }

        public void TestSkipWhile() {
            Func<int, int, int> f = (a, b) => Enumerable.Range(a, 50).SkipWhile(x => x < b).Sum();
            this.Test(f);
        }

        public void TestSkipWhileInt() {
            Func<int, int, int, int> f = (a, b, c) => Enumerable.Range(a, 50).SkipWhile((x, i) => x < b && i < c).Sum();
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

        #region Take, TakeWhile

        [Test]
        public void TestTake() {
            Func<int, int> f = a => Enumerable.Range(0, 50).Take(a).Sum();
            this.Test(f);
        }

        [Test]
        public void TestTakeWhile() {
            Func<int, int, int> f = (a, b) => Enumerable.Range(a, 50).TakeWhile(x => x < b).Sum();
            this.Test(f);
        }

        [Test]
        public void TestTakeWhileInt() {
            Func<int, int, int, int> f = (a, b, c) => Enumerable.Range(a, 50).TakeWhile((x, i) => x < b && i < c).Sum();
            this.Test(f);
        }

        #endregion

        #region Union

        [Test]
        public void TestUnionUnique() {
            Func<bool> f = () => {
                var a1 = new int[] { 0, 1 };
                var a2 = new int[] { 2, 3 };
                var r = a1.Union(a2).ToArray();
                return r[0] == 0 && r[1] == 1 && r[2] == 2 && r[3] == 3;
            };
            this.Test(f, true);
        }

        [Test]
        public void TestUnionOverlap() {
            Func<bool> f = () => {
                var a1 = new int[] { 0, 1, 2, 3, 2, 1, 0 };
                var a2 = new int[] { 0, 4, 1, 5, 3, 2, 1, 0, 6 };
                var r = a1.Union(a2);
                var expected = new int[] { 0, 1, 2, 3, 4, 5, 6 };
                return r.SequenceEqual(expected);
            };
            this.Test(f, true);
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

        #region Zip

        [Test]
        public void TestZip() {
            Func<bool> f = () => {
                var a1 = new int[] { 0, 1 };
                var a2 = new string[] { "zero", "one" };
                var r = a1.Zip(a2, (a, b) => new { a, b }).ToArray();
                return r.Length == 2 && r[0].a == 0 && r[0].b == "zero" && r[1].a == 1 && r[1].b == "one";
            };
            this.Test(f, true);
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

        #region Empty, Range, SequenceEqual

        [Test]
        public void TestEmpty() {
            Func<int> f = () => Enumerable.Empty<int>().Count();
            this.Test(f);
        }

        [Test]
        public void TestRangeCount() {
            Func<int> f = () => Enumerable.Range(0, 10).Count();
            this.Test(f);
        }

        [Test]
        public void TestRangeValues() {
            Func<bool> f = () => {
                var r = Enumerable.Range(3, 4).ToArray();
                return r[0] == 3 && r[1] == 4 && r[2] == 5 && r[3] == 6;
            };
            this.Test(f, true);
        }

        [Test]
        public void TestSequenceEqualOk() {
            Func<bool> f = () => {
                var a1 = new int[] { 1, 2, 3 };
                var a2 = new int[] { 1, 2, 3 };
                return a1.SequenceEqual(a2);
            };
            this.Test(f, true);
        }

        [Test]
        public void TestSequenceEqualOkEmpty() {
            Func<bool> f = () => {
                var a1 = Enumerable.Empty<int>();
                var a2 = Enumerable.Empty<int>();
                return a1.SequenceEqual(a2);
            };
            this.Test(f, true);
        }

        [Test]
        public void TestSequenceEqualBadValues() {
            Func<bool> f = () => {
                var a1 = new int[] { 1, 2, 3 };
                var a2 = new int[] { 1, 2, 4 };
                return a1.SequenceEqual(a2);
            };
            this.Test(f, false);
        }

        [Test]
        public void TestSequenceEqualBadLength() {
            Func<bool> f = () => {
                var a1 = new int[] { 1, 2, 3 };
                var a2 = new int[] { };
                return a1.SequenceEqual(a2);
            };
            this.Test(f, false);
        }

        #endregion

    }

}
