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

        [Test, Ignore("By-ref parameters not yet supported")]
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

    }

}
