using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestNullables : ExecutionTestBase {

        [Test]
        public void TestJustNull() {
            Func<bool> f = () => {
                int? i = null;
                return !i.HasValue;
            };
            this.Test(f);
        }

        [Test, Ignore("Value types not yet supported")]
        public void TestJustNotNull() {
            Func<bool> f = () => {
                int? i = 3;
                return i.HasValue;
            };
            this.Test(f);
        }

    }

}
