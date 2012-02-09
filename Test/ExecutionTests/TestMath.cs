using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestMath : ExecutionTestBase {

        [Test]
        public void TestSqrt() {
            Func<double> f = () => Math.Sqrt(4.0);
            this.Test(f);
        }

        [Test]
        public void TestTan() {
            Func<double> f = () => Math.Tan(1.0);
            this.Test(f);
        }

    }

}
