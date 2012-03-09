using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestHashSet : ExecutionTestBase {

        [Test]
        public void TestAddCheck() {
            Func<int, int, int> f = (a, b) => {
                var h = new HashSet<int>();
                var a1 = h.Add(a);
                var a2 = h.Add(a);
                var a3 = h.Add(b);
                var a4 = h.Add(b);
                return h.Count * 100 + (h.Contains(a) ? 10 : -10) + (h.Contains(a + 1) ? 1 : -1)
                    + (a1 ? 9 : -9) + (a2 ? 19 : -19) + (a3 ? 199 : -199) + (a4 ? 1999 : -1999);
            };
            this.Test(f);
        }

        [Test]
        public void TestEnumerator() {
            Func<int, int, int> f = (a, b) => {
                var h = new HashSet<int> { a, b };
                var ret = 0;
                foreach (var i in h) {
                    ret += i;
                }
                return ret;
            };
            this.Test(f);
        }

    }

}
