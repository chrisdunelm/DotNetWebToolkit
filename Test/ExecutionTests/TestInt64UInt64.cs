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

        [Test]
        public void TestInt64BitwiseNot() {
            this.Test((Func<Int64, Int64>)TestInt64BitwiseNotFunc);
        }
        private static Int64 TestInt64BitwiseNotFunc([ParamFullRange]Int64 a) {
            return ~a;
        }

        [Test]
        public void TestUInt64BitwiseNot() {
            this.Test((Func<UInt64, UInt64>)TestUInt64BitwiseNotFunc);
        }
        private static UInt64 TestUInt64BitwiseNotFunc([ParamFullRange]UInt64 a) {
            return ~a;
        }

        [Test]
        public void TestInt64BitwiseAnd() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64BitwiseAndFunc);
        }
        private static Int64 TestInt64BitwiseAndFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a & b;
        }

        [Test]
        public void TestUInt64BitwiseAnd() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64BitwiseAndFunc);
        }
        private static UInt64 TestUInt64BitwiseAndFunc([ParamFullRange]UInt64 a, [ParamFullRange]UInt64 b) {
            return a & b;
        }

        [Test]
        public void TestInt64BitwiseOr() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64BitwiseOrFunc);
        }
        private static Int64 TestInt64BitwiseOrFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a | b;
        }

        [Test]
        public void TestUInt64BitwiseOr() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64BitwiseOrFunc);
        }
        private static UInt64 TestUInt64BitwiseOrFunc([ParamFullRange]UInt64 a, [ParamFullRange]UInt64 b) {
            return a | b;
        }

        [Test]
        public void TestInt64BitwiseXor() {
            this.Test((Func<Int64, Int64, Int64>)TestInt64BitwiseXorFunc);
        }
        private static Int64 TestInt64BitwiseXorFunc([ParamFullRange]Int64 a, [ParamFullRange]Int64 b) {
            return a ^ b;
        }

        [Test]
        public void TestUInt64BitwiseXor() {
            this.Test((Func<UInt64, UInt64, UInt64>)TestUInt64BitwiseXorFunc);
        }
        private static UInt64 TestUInt64BitwiseXorFunc([ParamFullRange]UInt64 a, [ParamFullRange]UInt64 b) {
            return a ^ b;
        }

        [Test]
        public void TestInt64Equals() {
            Func<Int64, Int64, bool> f = (a, b) => a == b;
            this.Test(f);
        }

        [Test]
        public void TestUInt64Equals() {
            Func<UInt64, UInt64, bool> f = (a, b) => a == b;
            this.Test(f);
        }

        [Test]
        public void TestInt64NotEquals() {
            Func<Int64, Int64, bool> f = (a, b) => a != b;
            this.Test(f);
        }

        [Test]
        public void TestUInt64NotEquals() {
            Func<UInt64, UInt64, bool> f = (a, b) => a != b;
            this.Test(f);
        }

        [Test]
        public void TestInt64LessThan() {
            Func<Int64, Int64, bool> f = (a, b) => a < b;
            this.Test(f);
        }

        [Test]
        public void TestUInt64LessThan() {
            Func<UInt64, UInt64, bool> f = (a, b) => a < b;
            this.Test(f);
        }

        [Test]
        public void TestInt64LessThanOrEqual() {
            Func<Int64, Int64, bool> f = (a, b) => a <= b;
            this.Test(f);
        }

        [Test]
        public void TestUInt64LessThanOrEqual() {
            Func<UInt64, UInt64, bool> f = (a, b) => a <= b;
            this.Test(f);
        }

        [Test]
        public void TestInt64GreaterThan() {
            Func<Int64, Int64, bool> f = (a, b) => a > b;
            this.Test(f);
        }

        [Test]
        public void TestUInt64GreaterThan() {
            Func<UInt64, UInt64, bool> f = (a, b) => a > b;
            this.Test(f);
        }

        [Test]
        public void TestInt64GreaterThanOrEqual() {
            Func<Int64, Int64, bool> f = (a, b) => a >= b;
            this.Test(f);
        }

        [Test]
        public void TestUInt64GreaterThanOrEqual() {
            Func<UInt64, UInt64, bool> f = (a, b) => a >= b;
            this.Test(f);
        }

        private static Int64 ReturnInt64Inc(Int64 a) {
            a++;
            return a;
        }
        [Test]
        public void TestInt64ValueType() {
            Func<Int64, Int64> f = a => {
                var b = ReturnInt64Inc(a);
                return a + b;
            };
            this.Test(f);
        }

    }

}
