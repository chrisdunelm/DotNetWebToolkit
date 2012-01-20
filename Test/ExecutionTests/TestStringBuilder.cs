using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestStringBuilder : ExecutionTestBase {

        [Test, Ignore("StringBuilder not yet supported (uses lots of unsafe code)")]
        public void TestToStringOnly() {
            Func<string> f = () => (new StringBuilder()).ToString();
            this.Test(f);
        }

    }

}
