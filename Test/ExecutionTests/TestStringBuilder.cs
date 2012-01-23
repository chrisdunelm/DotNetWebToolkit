using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestStringBuilder : ExecutionTestBase {

        [Test]
        public void TestToStringOnly() {
            Func<string> f = () => (new StringBuilder()).ToString();
            this.Test(f);
        }

        [Test]
        public void TestAppendString() {
            Func<string, string> f = a => (new StringBuilder()).Append(a).Append(a).ToString();
            this.Test(f);
        }

    }

}
