using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestConv : ExecutionTestBase {

        [Test]
        public void TestSByteToByte() {
            this.Test((Func<SByte, Byte>)TestSByteToByteFunc);
        }
        private static Byte TestSByteToByteFunc([ParamFullRange]SByte v) {
            return (Byte)v;
        }

        [Test]
        public void TestSByteToInt16() {
            this.Test((Func<SByte, Int16>)TestSByteToInt16Func);
        }
        private static Int16 TestSByteToInt16Func([ParamFullRange]SByte v) {
            return (Int16)v;
        }

        [Test]
        public void TestSByteToInt32() {
            this.Test((Func<SByte, Int32>)TestSByteToInt32Func);
        }
        private static Int32 TestSByteToInt32Func([ParamFullRange]SByte v) {
            return (Int32)v;
        }

        [Test]
        public void TestSByteToInt64() {
            this.Test((Func<SByte, Int64>)TestSByteToInt64Func);
        }
        private static Int64 TestSByteToInt64Func([ParamFullRange]SByte v) {
            return (Int64)v;
        }

        [Test]
        public void TestSByteToUInt16() {
            this.Test((Func<SByte, UInt16>)TestSByteToUInt16Func);
        }
        private static UInt16 TestSByteToUInt16Func([ParamFullRange]SByte v) {
            return (UInt16)v;
        }

        [Test]
        public void TestSByteToUInt32() {
            this.Test((Func<SByte, UInt32>)TestSByteToUInt32Func);
        }
        private static UInt32 TestSByteToUInt32Func([ParamFullRange]SByte v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestSByteToUInt64() {
            this.Test((Func<SByte, UInt64>)TestSByteToUInt64Func);
        }
        private static UInt64 TestSByteToUInt64Func([ParamFullRange]SByte v) {
            return (UInt64)v;
        }

        [Test]
        public void TestSByteToSingle() {
            this.Test((Func<SByte, Single>)TestSByteToSingleFunc);
        }
        private static Single TestSByteToSingleFunc([ParamFullRange]SByte v) {
            return (Single)v;
        }

        [Test]
        public void TestSByteToDouble() {
            this.Test((Func<SByte, Double>)TestSByteToDoubleFunc);
        }
        private static Double TestSByteToDoubleFunc([ParamFullRange]SByte v) {
            return (Double)v;
        }

        [Test]
        public void TestByteToSByte() {
            this.Test((Func<Byte, SByte>)TestByteToSByteFunc);
        }
        private static SByte TestByteToSByteFunc([ParamFullRange]Byte v) {
            return (SByte)v;
        }

        [Test]
        public void TestByteToInt16() {
            this.Test((Func<Byte, Int16>)TestByteToInt16Func);
        }
        private static Int16 TestByteToInt16Func([ParamFullRange]Byte v) {
            return (Int16)v;
        }

        [Test]
        public void TestByteToInt32() {
            this.Test((Func<Byte, Int32>)TestByteToInt32Func);
        }
        private static Int32 TestByteToInt32Func([ParamFullRange]Byte v) {
            return (Int32)v;
        }

        [Test]
        public void TestByteToInt64() {
            this.Test((Func<Byte, Int64>)TestByteToInt64Func);
        }
        private static Int64 TestByteToInt64Func([ParamFullRange]Byte v) {
            return (Int64)v;
        }

        [Test]
        public void TestByteToUInt16() {
            this.Test((Func<Byte, UInt16>)TestByteToUInt16Func);
        }
        private static UInt16 TestByteToUInt16Func([ParamFullRange]Byte v) {
            return (UInt16)v;
        }

        [Test]
        public void TestByteToUInt32() {
            this.Test((Func<Byte, UInt32>)TestByteToUInt32Func);
        }
        private static UInt32 TestByteToUInt32Func([ParamFullRange]Byte v) {
            return (UInt32)v;
        }

        [Test]
        public void TestByteToUInt64() {
            this.Test((Func<Byte, UInt64>)TestByteToUInt64Func);
        }
        private static UInt64 TestByteToUInt64Func([ParamFullRange]Byte v) {
            return (UInt64)v;
        }

        [Test]
        public void TestByteToSingle() {
            this.Test((Func<Byte, Single>)TestByteToSingleFunc);
        }
        private static Single TestByteToSingleFunc([ParamFullRange]Byte v) {
            return (Single)v;
        }

        [Test]
        public void TestByteToDouble() {
            this.Test((Func<Byte, Double>)TestByteToDoubleFunc);
        }
        private static Double TestByteToDoubleFunc([ParamFullRange]Byte v) {
            return (Double)v;
        }

        [Test]
        public void TestInt16ToSByte() {
            this.Test((Func<Int16, SByte>)TestInt16ToSByteFunc);
        }
        private static SByte TestInt16ToSByteFunc([ParamFullRange]Int16 v) {
            return (SByte)v;
        }

        [Test]
        public void TestInt16ToByte() {
            this.Test((Func<Int16, Byte>)TestInt16ToByteFunc);
        }
        private static Byte TestInt16ToByteFunc([ParamFullRange]Int16 v) {
            return (Byte)v;
        }

        [Test]
        public void TestInt16ToInt32() {
            this.Test((Func<Int16, Int32>)TestInt16ToInt32Func);
        }
        private static Int32 TestInt16ToInt32Func([ParamFullRange]Int16 v) {
            return (Int32)v;
        }

        [Test]
        public void TestInt16ToInt64() {
            this.Test((Func<Int16, Int64>)TestInt16ToInt64Func);
        }
        private static Int64 TestInt16ToInt64Func([ParamFullRange]Int16 v) {
            return (Int64)v;
        }

        [Test]
        public void TestInt16ToUInt16() {
            this.Test((Func<Int16, UInt16>)TestInt16ToUInt16Func);
        }
        private static UInt16 TestInt16ToUInt16Func([ParamFullRange]Int16 v) {
            return (UInt16)v;
        }

        [Test]
        public void TestInt16ToUInt32() {
            this.Test((Func<Int16, UInt32>)TestInt16ToUInt32Func);
        }
        private static UInt32 TestInt16ToUInt32Func([ParamFullRange]Int16 v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt16ToUInt64() {
            this.Test((Func<Int16, UInt64>)TestInt16ToUInt64Func);
        }
        private static UInt64 TestInt16ToUInt64Func([ParamFullRange]Int16 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestInt16ToSingle() {
            this.Test((Func<Int16, Single>)TestInt16ToSingleFunc);
        }
        private static Single TestInt16ToSingleFunc([ParamFullRange]Int16 v) {
            return (Single)v;
        }

        [Test]
        public void TestInt16ToDouble() {
            this.Test((Func<Int16, Double>)TestInt16ToDoubleFunc);
        }
        private static Double TestInt16ToDoubleFunc([ParamFullRange]Int16 v) {
            return (Double)v;
        }

        [Test]
        public void TestInt32ToSByte() {
            this.Test((Func<Int32, SByte>)TestInt32ToSByteFunc);
        }
        private static SByte TestInt32ToSByteFunc([ParamFullRange]Int32 v) {
            return (SByte)v;
        }

        [Test]
        public void TestInt32ToByte() {
            this.Test((Func<Int32, Byte>)TestInt32ToByteFunc);
        }
        private static Byte TestInt32ToByteFunc([ParamFullRange]Int32 v) {
            return (Byte)v;
        }

        [Test]
        public void TestInt32ToInt16() {
            this.Test((Func<Int32, Int16>)TestInt32ToInt16Func);
        }
        private static Int16 TestInt32ToInt16Func([ParamFullRange]Int32 v) {
            return (Int16)v;
        }

        [Test]
        public void TestInt32ToInt64() {
            this.Test((Func<Int32, Int64>)TestInt32ToInt64Func);
        }
        private static Int64 TestInt32ToInt64Func([ParamFullRange]Int32 v) {
            return (Int64)v;
        }

        [Test]
        public void TestInt32ToUInt16() {
            this.Test((Func<Int32, UInt16>)TestInt32ToUInt16Func);
        }
        private static UInt16 TestInt32ToUInt16Func([ParamFullRange]Int32 v) {
            return (UInt16)v;
        }

        [Test]
        public void TestInt32ToUInt32() {
            this.Test((Func<Int32, UInt32>)TestInt32ToUInt32Func);
        }
        private static UInt32 TestInt32ToUInt32Func([ParamFullRange]Int32 v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt32ToUInt64() {
            this.Test((Func<Int32, UInt64>)TestInt32ToUInt64Func);
        }
        private static UInt64 TestInt32ToUInt64Func([ParamFullRange]Int32 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestInt32ToSingle() {
            this.Test((Func<Int32, Single>)TestInt32ToSingleFunc);
        }
        private static Single TestInt32ToSingleFunc([ParamFullRange]Int32 v) {
            return (Single)v;
        }

        [Test]
        public void TestInt32ToDouble() {
            this.Test((Func<Int32, Double>)TestInt32ToDoubleFunc);
        }
        private static Double TestInt32ToDoubleFunc([ParamFullRange]Int32 v) {
            return (Double)v;
        }

    }

}
