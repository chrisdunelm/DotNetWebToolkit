using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

using Int8 = System.SByte;
using UInt8 = System.Byte;

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
        public void TestCos() {
            Func<double> f = () => Math.Cos(1.0);
            this.Test(f);
        }

        [Test]
        public void TestMaxInt8() {
            Func<Int8, Int8, Int8> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxInt16() {
            Func<Int16, Int16, Int16> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxInt32() {
            Func<Int32, Int32, Int32> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxInt64() {
            Func<Int64, Int64, Int64> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxUInt8() {
            Func<UInt8, UInt8, UInt8> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxUInt16() {
            Func<UInt16, UInt16, UInt16> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxUInt32() {
            Func<UInt32, UInt32, UInt32> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxUInt64() {
            Func<UInt64, UInt64, UInt64> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxSingle() {
            Func<float, float, float> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMaxDouble() {
            Func<double, double, double> f = (a, b) => Math.Max(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinInt8() {
            Func<Int8, Int8, Int8> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinInt16() {
            Func<Int16, Int16, Int16> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinInt32() {
            Func<Int32, Int32, Int32> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinInt64() {
            Func<Int64, Int64, Int64> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinUInt8() {
            Func<UInt8, UInt8, UInt8> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinUInt16() {
            Func<UInt16, UInt16, UInt16> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinUInt32() {
            Func<UInt32, UInt32, UInt32> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinUInt64() {
            Func<UInt64, UInt64, UInt64> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinSingle() {
            Func<float, float, float> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestMinDouble() {
            Func<double, double, double> f = (a, b) => Math.Min(a, b);
            this.Test(f);
        }

        [Test]
        public void TestPow() {
            Func<double, double, double> f = (a, b) => Math.Pow(a - 50, b - 50);
            this.Test(f);
        }

        [Test]
        public void TestSignInt8() {
            Func<Int8, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignInt16() {
            Func<Int16, int> f = a => Math.Sign(a - 50);
            this.Test(f);
        }

        [Test]
        public void TestSignInt32() {
            Func<Int32, int> f = a => Math.Sign(a - 50);
            this.Test(f);
        }

        [Test]
        public void TestSignInt64() {
            Func<Int64, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignIntSingle() {
            Func<Single, int> f = a => Math.Sign(a - 50);
            this.Test(f);
        }

        [Test]
        public void TestSignDouble() {
            Func<Double, int> f = a => Math.Sign(a - 50);
            this.Test(f);
        }

        [Test]
        public void TestSin() {
            Func<double> f = () => Math.Sin(1.0);
            this.Test(f);
        }

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
