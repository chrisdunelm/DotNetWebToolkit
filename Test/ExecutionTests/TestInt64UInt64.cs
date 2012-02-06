using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestInt64UInt64 : ExecutionTestBase {

        [Test]
        public void TestSimpleReturnLargePos() {
            Func<Int64> f = () => 0x100000000L;
            this.Test(f);
        }

        [Test]
        public void TestSimpleReturnLargeNeg() {
            Func<Int64> f = () => -0x100000000L;
            this.Test(f);
        }

        [Test]
        public void TestAdd() {
            this.Test((Func<Int64, Int64, Int64>)TestAddFunc);
        }
        private static Int64 TestAddFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a + b;
        }

        [Test]
        public void TestSubtract() {
            this.Test((Func<Int64, Int64, Int64>)TestSubtractFunc);
        }
        private static Int64 TestSubtractFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a - b;
        }

        [Test, Ignore("Int64 not complete")]
        public void TestMultiply() {
            this.Test((Func<Int64, Int64, Int64>)TestMultiplyFunc);
        }
        private static Int64 TestMultiplyFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a * b;
        }

        [Test, Ignore("Int64 not complete")]
        public void TestDivide() {
            this.Test((Func<Int64, Int64, Int64>)TestDivideFunc);
        }
        private static Int64 TestDivideFunc([ParamFullRange]Int64 a, [ParamFullRangeNonZero]Int64 b) {
            return a / b;
        }

    }

}
