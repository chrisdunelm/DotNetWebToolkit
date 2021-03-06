﻿using System;
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
    [ParamFullRange]
    public class TestMath : ExecutionTestBase {

        [Test]
        public void TestAbsInt8() {
            Func<Int8, Int8> f = a => Math.Abs(a);
            this.Test(f);
        }

        [Test]
        public void TestAbsInt16() {
            Func<Int16, Int16> f = a => Math.Abs(a);
            this.Test(f);
        }

        [Test]
        public void TestAbsInt8and16() {
            Func<Int16, Int32> f = a => Math.Abs((Int8)a) + Math.Abs((Int16)a);
            this.Test(f);
        }

        [Test]
        public void TestAbsInt32() {
            Func<int, int> f = a => Math.Abs(a);
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
        [WithinPercent(0.01)]
        private static float TestAbsSingleFunc(float a) {
            return Math.Abs(a);
        }

        [Test]
        public void TestAbsDouble() {
            Func<double, double> f = a => Math.Abs(a);
            this.Test(f);
        }

        [Test]
        public void TestAcos() {
            Func<double, double> f = a => Math.Acos(a);
            this.Test(f);
        }

        [Test]
        public void TestAsin() {
            Func<double, double> f = a => Math.Asin(a);
            this.Test(f);
        }

        [Test]
        public void TestAtan() {
            Func<double, double> f = a => Math.Atan(a);
            this.Test(f);
        }

        [Test]
        public void TestAtan2() {
            Func<double, double, double> f = (a, b) => Math.Atan2(a, b);
            this.Test(f);
            this.Test((Func<double>)(() => Math.Atan2(0.5, 0.5)));
            this.Test((Func<double>)(() => Math.Atan2(0.5, -0.5)));
            this.Test((Func<double>)(() => Math.Atan2(-0.5, -0.5)));
            this.Test((Func<double>)(() => Math.Atan2(-0.5, 0.5)));
            this.Test((Func<double>)(() => Math.Atan2(0, 1)));
            this.Test((Func<double>)(() => Math.Atan2(0, -1)));
            this.Test((Func<double>)(() => Math.Atan2(1, 0)));
            this.Test((Func<double>)(() => Math.Atan2(-1, 0)));
        }

        [Test]
        public void TestBigMul() {
            Func<long, long, long> f = (a, b) => Math.BigMul((int)a, (int)b);
            this.Test(f);
        }

        [Test]
        public void TestCeiling() {
            Func<double, double> f = a => Math.Ceiling(a);
            this.Test(f);
        }

        [Test]
        public void TestCos() {
            Func<double, double> f = a => Math.Cos(a);
            this.Test(f);
        }

        [Test]
        public void TestCosh() {
            Func<double, double> f = a => Math.Cosh(a);
            this.Test(f);
        }

        [Test]
        public void TestDivRemInt32() {
            Func<int, int, int> f = (a, b) => {
                int r;
                int q = Math.DivRem(a, b == 0 ? 1 : b, out r);
                return q + r;
            };
            this.Test(f);
        }

        [Test]
        public void TestDivRemInt64() {
            Func<long,long,long> f = (a, b) => {
                long r;
                long q = Math.DivRem(a, b == 0 ? 1 : b, out r);
                return q + r;
            };
            this.Test(f);
        }

        [Test]
        public void TestExp() {
            Func<double, double> f = a => Math.Exp(a);
            this.Test(f);
        }

        [Test]
        public void TestFloor() {
            Func<double, double> f = a => Math.Floor(a);
            this.Test(f);
        }

        [Test]
        public void TestLogE() {
            Func<double, double> f = a => Math.Log(a);
            this.Test(f);
        }

        [Test]
        public void TestLogAnyBase() {
            Func<double, double, double> f = (a,b) => Math.Log(a, b);
            this.Test(f);
        }

        [Test]
        public void TestLog10() {
            Func<double, double> f = a => Math.Log10(a);
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
            this.Test((Func<double, double, double>)TestPowFunc);
        }
        [WithinPercent(0.01)]
        private static double TestPowFunc(double a, double b) {
            return Math.Pow(a, b);
        }

        [Test]
        public void TestRound() {
            Func<double, double> f = a => Math.Round(a);
            this.Test(f);
        }

        [Test]
        public void TestRoundDigits() {
            Func<double, int, double> f = (a, b) => {
                if (double.IsInfinity(a) || Math.Abs(a) < double.MaxValue / 1e20) {
                    return Math.Round(a, b & 0xf);
                } else {
                    return 0;
                }
            };
            this.Test(f);
        }

        [Test]
        public void TestSignInt8() {
            Func<Int8, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignInt16() {
            Func<Int16, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignInt32() {
            Func<Int32, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignInt64() {
            Func<Int64, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignIntSingle() {
            Func<Single, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSignDouble() {
            Func<Double, int> f = a => Math.Sign(a);
            this.Test(f);
        }

        [Test]
        public void TestSin() {
            Func<double, double> f = a => Math.Sin(a);
            this.Test(f);
        }

        [Test]
        public void TestSinh() {
            this.Test((Func<double, double>)TestSinhFunc);
        }
        [Within(0.000001)]
        private static double TestSinhFunc(double a) {
            return Math.Sinh(a);
        }

        [Test]
        public void TestSqrt() {
            Func<double, double> f = a => Math.Sqrt(a);
            this.Test(f);
        }

        [Test]
        public void TestTan() {
            Func<double, double> f = a => Math.Tan(a);
            this.Test(f);
        }

        [Test]
        public void TestTanh() {
            this.Test((Func<double, double>)TestTanhFunc);
        }
        [Within(0.000001)]
        private static double TestTanhFunc(double a) {
            return Math.Tanh(a);
        }

        [Test]
        public void TestTruncate() {
            Func<double, double> f = a => Math.Truncate(a);
            this.Test(f);
        }

    }

}
