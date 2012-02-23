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

    }

}
