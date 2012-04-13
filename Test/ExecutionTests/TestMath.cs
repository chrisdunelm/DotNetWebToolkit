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
        public void TestAbsInt32() {
            Func<int, int> f = a => Math.Abs(a - 50);
            this.Test(f);
        }

        [Test]
        public void TestAbsDouble() {
            Func<double, double> f = a => Math.Abs(a);
            this.Test(f);
        }

        [Test]
        public void TestSqrt() {
            Func<double> f = () => Math.Sqrt(4.0);
            this.Test(f);
        }

        [Test]
        public void TestSin() {
            Func<double> f = () => Math.Sin(1.0);
            this.Test(f);
        }

        [Test]
        public void TestCos() {
            Func<double> f = () => Math.Cos(1.0);
            this.Test(f);
        }

        [Test]
        public void TestTan() {
            Func<double> f = () => Math.Tan(1.0);
            this.Test(f);
        }

    }

}
