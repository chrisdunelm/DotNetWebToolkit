using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestTypes : ExecutionTestBase {

        [Test]
        public void TestDouble() {
            Func<string> f = () => {
                var d = 0.1;
                if (d < 0.0) {
                    return "<";
                } else {
                    return ">";
                }
            };
            this.Test(f);
        }

        static UInt32 GetLargeUInt32() {
            return UInt32.MaxValue;
        }
        [Test]
        public void TestUInt32CompareLargeToLiteral() {
            Func<bool> f = () => GetLargeUInt32() == 0xffffffff;
            this.Test(f, true);
        }

    }

}
