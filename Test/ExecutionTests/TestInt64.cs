using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestInt64 : ExecutionTestBase {

        [Test]
        public void TestSimpleReturnLargePos() {
            Func<Int64> f = () => 0x100000000L;
            this.Test(f);
        }

        [Test]
        public void TestSimpleReturnLargeNeg() {
            Func<Int64> f = () => -0x100000000L;
            this.Test(f);
        }

    }

}
