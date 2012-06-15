using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using DotNetWebToolkit.Cil2Js.Utils;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestArithmetic : ExecutionTestBase {

        [Test]
        public void TestAdd() {
            Func<int, int, int> f = (a, b) => a + b;
            this.Test(f);
        }

        [Test]
        public void TestSub() {
            Func<int, int, int> f = (a, b) => a - b;
            this.Test(f);
        }

        [Test]
        public void TestMul() {
            Func<int, int, int> f = (a, b) => a * b;
            this.Test(f);
        }

        [Test]
        public void TestIntegerDiv() {
            this.Test((Func<int, int, int>)TestIntegerDivFunc);
        }
        [IterationCount(50)]
        private static int TestIntegerDivFunc([ParamAny]int a, [ParamNonZero]int b) {
            return a / b;
        }

        [Test]
        public void TestDoubleDiv() {
            this.Test((Func<double, double, double>)TestDoubleDivFunc);
        }
        [IterationCount(50), Within(0.0001)]
        private static double TestDoubleDivFunc([ParamAny]double a, [ParamNonZero]double b) {
            return a / b;
        }

        [Test]
        public void TestIntRem() {
            this.Test((Func<int, int, int>)TestIntRemFunc);
        }
        [IterationCount(50)]
        private static int TestIntRemFunc([ParamAny]int a, [ParamNonZero]int b) {
            return a % b;
        }

        [Test]
        public void TestDoubleRem() {
            this.Test((Func<double, double, double>)TestDoubleRemFunc);
        }
        [IterationCount(50), Within(0.0001)]
        private static double TestDoubleRemFunc([ParamAny]double a, [ParamNonZero]double b) {
            return a % b;
        }

        [Test]
        public void TestBitwiseAnd() {
            Func<int, int, int> f = (a, b) => a & b;
            this.Test(f);
        }

        [Test]
        public void TestBitwiseOr() {
            Func<int, int, int> f = (a, b) => a | b;
            this.Test(f);
        }

        [Test]
        public void TestBitwiseXor() {
            Func<int, int, int> f = (a, b) => a ^ b;
            this.Test(f);
        }

        [Test]
        public void TestSingleIsNaN() {
            this.Test((Func<Single, bool>)TestSingleIsNaNFunc);
        }
        private static bool TestSingleIsNaNFunc([ParamFullRange]Single a) {
            return Single.IsNaN(a);
        }

        [Test]
        public void TestDoubleIsNaN() {
            this.Test((Func<Double, bool>)TestDoubleIsNaNFunc);
        }
        private static bool TestDoubleIsNaNFunc([ParamFullRange]Double a) {
            return Double.IsNaN(a);
        }

        [Test]
        public void TestSingleIsNegativeInfinity() {
            this.Test((Func<Single, bool>)TestSingleIsNegativeInfinityFunc);
        }
        public static bool TestSingleIsNegativeInfinityFunc([ParamFullRange]Single a) {
            return Single.IsNegativeInfinity(a);
        }

        [Test]
        public void TestSingleIsPositiveInfinity() {
            this.Test((Func<Single, bool>)TestSingleIsPositiveInfinityFunc);
        }
        public static bool TestSingleIsPositiveInfinityFunc([ParamFullRange]Single a) {
            return Single.IsNegativeInfinity(a);
        }

        [Test]
        public void TestSingleIsInfinity() {
            this.Test((Func<Single, bool>)TestSingleIsInfinityFunc);
        }
        private static bool TestSingleIsInfinityFunc([ParamFullRange]Single a) {
            return Single.IsInfinity(a);
        }

        [Test]
        public void TestDoubleIsNegativeInfinity() {
            this.Test((Func<Double, bool>)TestDoubleIsNegativeInfinityFunc);
        }
        public static bool TestDoubleIsNegativeInfinityFunc([ParamFullRange]Double a) {
            return Double.IsNegativeInfinity(a);
        }

        [Test]
        public void TestDoubleIsPositiveInfinity() {
            this.Test((Func<Double, bool>)TestDoubleIsPositiveInfinityFunc);
        }
        public static bool TestDoubleIsPositiveInfinityFunc([ParamFullRange]Double a) {
            return Double.IsNegativeInfinity(a);
        }

        [Test]
        public void TestDoubleIsInfinity() {
            this.Test((Func<Double, bool>)TestDoubleIsInfinityFunc);
        }
        private static bool TestDoubleIsInfinityFunc([ParamFullRange]Double a) {
            return Double.IsInfinity(a);
        }

    }
}
