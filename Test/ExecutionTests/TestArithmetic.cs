using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil;
using Cil2Js;
using Cil2Js.Utils;
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
        [IterationCount(100)]
        private int TestIntegerDivFunc([ParamAny]int a, [ParamNonZero]int b) {
            return a / b;
        }

        [Test]
        public void TestDoubleDiv() {
            this.Test((Func<double, double, double>)TestDoubleDivFunc);
        }
        [IterationCount(100)]
        private double TestDoubleDivFunc([ParamAny]double a, [ParamNonZero]double b) {
            return a / b;
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

    }
}
