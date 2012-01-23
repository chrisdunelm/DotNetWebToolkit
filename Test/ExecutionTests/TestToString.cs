using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestToString : ExecutionTestBase {

        [Test]
        public void TestString() {
            Func<string, string> f = a => a.ToString();
            this.Test(f);
        }

        [Test, Ignore("Boolean ToString() not yet working")]
        public void TestBoolean() {
            Func<string> f = () => true.ToString() + false.ToString();
            this.Test(f);
        }

    }

}
