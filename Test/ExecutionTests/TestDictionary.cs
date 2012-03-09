using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestDictionary : ExecutionTestBase {

        [Test]
        public void TestAddRead() {
            Func<int, string, int> f = (i, s) => {
                var d = new Dictionary<string, int>();
                d.Add(s, i);
                return d[s];
            };
            this.Test(f);
        }

        [Test]
        public void TestTryGetValue() {
            Func<int, string, string> f = (i, s) => {
                var d = new Dictionary<int, string>();
                d.Add(i, s);
                string sOut;
                bool ok = d.TryGetValue(i | 1, out sOut);
                return ok ? sOut : "no";
            };
            this.Test(f);
        }

        [Test]
        public void TestContainsKey() {
            Func<int, bool> f = a => {
                var d = new Dictionary<int, object>();
                d.Add(a & 3, null);
                return d.ContainsKey(1);
            };
            this.Test(f);
        }

        [Test]
        public void TestOverwrite() {
            Func<int, int> f = a => {
                var d = new Dictionary<int, int>();
                d[0] = 0;
                d[0] = a;
                return d[0];
            };
            this.Test(f);
        }

        [Test]
        public void TestClear() {
            Func<int> f = () => {
                var d = new Dictionary<int, int>();
                d.Add(0, 0);
                d.Clear();
                return d.Count;
            };
            this.Test(f);
        }

        [Test]
        public void TestRemove() {
            Func<bool> f = () => {
                var d = new Dictionary<int, int>();
                d.Add(0, 0);
                d.Remove(0);
                int i;
                return d.TryGetValue(0, out i);
            };
            this.Test(f);
        }

        [Test]
        public void TestRemoveReinsertLarge() {
            Func<int, int> f = a => {
                var d = new Dictionary<int, int>();
                int ret = 0;
                for (int i = 0; i < 10; i++) {
                    for (int j = 0; j < a; j++) {
                        d.Add(j, j * 2);
                    }
                    for (int j = 10; j < a; j++) {
                        d.Remove(j);
                    }
                    for (int j = 20; j < a; j++) {
                        d.Add(j, j * 10);
                    }
                    for (int j = 0; j < a; j++) {
                        int v;
                        if (d.TryGetValue(j, out v)) {
                            ret += v;
                        }
                        ret += (d.Remove(j) ? 100 : 101) * j;
                    }
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestLarge() {
            Func<int, int> f = a => {
                var d = new Dictionary<int, int>();
                for (int i = 0; i < a; i++) {
                    d.Add(i, i * 2);
                }
                int ret = 0;
                for (int i = 0; i < a; i++) {
                    ret += d[i];
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestEnumerator() {
            Func<int, int, int> f = (a, b) => {
                var d = new Dictionary<int, int> {
                    { a, a * 2 },
                    { b, b * 3 },
                };
                var ret = 0;
                var en = d.GetEnumerator();
                while (en.MoveNext()) {
                    ret += en.Current.Key + en.Current.Value;
                }
                foreach (var e in d) {
                    ret += e.Key + e.Value;
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestKeys() {
            Func<int, int, int> f = (a, b) => {
                var d = new Dictionary<int, object>();
                d.Add(a, null);
                d.Add(b, null);
                var ret = d.Keys.Count * 1000;
                foreach (var k in d.Keys) {
                    ret += k;
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestValues() {
            Func<int, int, int> f = (a, b) => {
                var d = new Dictionary<int, int>();
                d.Add(a, a);
                d.Add(b, b);
                var ret = d.Values.Count * 1000;
                foreach (var k in d.Values) {
                    ret += k;
                }
                return ret;
            };
            this.Test(f);
        }

    }

}
