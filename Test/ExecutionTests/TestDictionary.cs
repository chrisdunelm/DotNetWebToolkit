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

    }

}
