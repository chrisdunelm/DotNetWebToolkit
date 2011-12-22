using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestClosures : ExecutionTestBase {

        [Test, Ignore("Generics not yet implemented")]
        public void TestSimple() {
            Func<int, int> f = a => {
                Func<int> c = () => a;
                return c();
            };
            this.Test(f);
        }

    }

}
