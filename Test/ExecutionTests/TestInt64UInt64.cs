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
        public void TestInt64ReturnMax() {
            Func<Int64> f = () => Int64.MaxValue;
            this.Test(f);
        }

        [Test]
        public void TestUInt64ReturnMax() {
            Func<UInt64> f = () => UInt64.MaxValue;
            this.Test(f);
        }

        [Test]
        public void TestInt64ReturnMin() {
            Func<Int64> f = () => Int64.MinValue;
            this.Test(f);
        }

        [Test]
        public void TestUInt64ReturnMin() {
            Func<UInt64> f = () => UInt64.MinValue;
            this.Test(f);
        }

        [Test]
        public void TestInt64Negate() {
            this.Test((Func<Int64, Int64>)TestInt64NegateFunc);
        }
        private static Int64 TestInt64NegateFunc([ParamFullRange]Int64 a) {
            return -a;
        }

        [Test]
        public void TestInt64Add() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64AddFunc);
        }
        private static Int64 TestInt64AddFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a + b;
        }

        [Test]
        public void TestUInt64Add() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64AddFunc);
        }
        private static UInt64 TestUInt64AddFunc([ParamFullRange]UInt64 a, [ParamFullRange]UInt64 b) {
            return a + b;
        }

        [Test]
        public void TestInt64Subtract() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64SubtractFunc);
        }
        private static Int64 TestInt64SubtractFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a - b;
        }

        [Test]
        public void TestUInt64Subtract() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64SubtractFunc);
        }
        private static UInt64 TestUInt64SubtractFunc([ParamFullRange]UInt64 a, [ParamFullRange]UInt64 b) {
            return a - b;
        }

        [Test]
        public void TestInt64Multiply() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64MultiplyFunc);
        }
        private static Int64 TestInt64MultiplyFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a * b;
        }

        [Test]
        public void TestInt64MultiplyManual() {
            Func<Int64, Int64> f = a => {
                return a * -3L;
            };
            this.Test(f);
        }

        [Test]
        public void TestUInt64Multiply() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64MultiplyFunc);
        }
        private static UInt64 TestUInt64MultiplyFunc([ParamFullRange]UInt64 a, [ParamFullRange]UInt64 b) {
            return a * b;
        }

        [Test]
        public void TestInt64Divide() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64DivideFunc);
        }
        private static Int64 TestInt64DivideFunc([ParamFullRange]Int64 a, [ParamFullRangeNonZero]Int64 b) {
            return a / b;
        }

        [Test]
        public void TestUInt64Divide() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64DivideFunc);
            Func<UInt64, UInt64> f = a => {
                return a / 0x100000000UL;
            };
            this.Test(f);
        }
        private static UInt64 TestUInt64DivideFunc([ParamFullRange]UInt64 a, [ParamFullRangeNonZero]UInt64 b) {
            return a / b;
        }

        [Test]
        public void TestInt64Remainder() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64RemainderFunc);
        }
        private static Int64 TestInt64RemainderFunc([ParamFullRange(Int64.MinValue + 1, Int64.MaxValue)]Int64 a, [ParamFullRangeNonZero(Int64.MinValue + 1, Int64.MaxValue)]Int64 b) {
            return a % b;
        }

        [Test]
        public void TestUInt64Remainder() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestInt64RemainderFunc);
        }
        private static UInt64 TestInt64RemainderFunc([ParamFullRange]UInt64 a, [ParamFullRangeNonZero]UInt64 b) {
            return a % b;
        }

    }

}
