using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;
using Int8 = System.SByte;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestMath : ExecutionTestBase {

        [Test]
        public void TestAbsInt8() {
            Func<Int8, Int8> f = a => Math.Abs(a);
            this.Test(f);
        }

        [Test]
        public void TestAbsInt16() {
            Func<Int16, Int16> f = a => Math.Abs((Int16)(a - 50));
            this.Test(f);
        }

        [Test]
        public void TestAbsInt32() {
            Func<int, int> f = a => Math.Abs(a - 50);
            this.Test(f);
        }

        [Test]
        public void TestAbsInt64() {
            Func<Int64, Int64> f = a => Math.Abs(a);
            this.Test(f);
        }

        [Test]
        public void TestAbsSingle() {
            this.Test((Func<float, float>)TestAbsSingleFunc);
        }
        [Within(0.0001)]
        private static float TestAbsSingleFunc(float a) {
            return Math.Abs(a - 50);
        }

        [Test]
        public void TestAbsDouble() {
            Func<double, double> f = a => Math.Abs(a - 50);
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
