using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestType : ExecutionTestBase {

        [Test]
        public void TestTypeToString() {
            Func<string> f = () => typeof(int).ToString();
            this.Test(f);
        }

    }

}
