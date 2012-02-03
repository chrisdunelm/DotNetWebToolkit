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

        [Test, Ignore("64-bit values not yet properly supported")]
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

        [Test, Ignore("64-bit values not yet properly supported")]
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

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToSByte() {
            this.Test((Func<Int64, SByte>)TestInt64ToSByteFunc);
        }
        private static SByte TestInt64ToSByteFunc([ParamFullRange]Int64 v) {
            return (SByte)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToByte() {
            this.Test((Func<Int64, Byte>)TestInt64ToByteFunc);
        }
        private static Byte TestInt64ToByteFunc([ParamFullRange]Int64 v) {
            return (Byte)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToInt16() {
            this.Test((Func<Int64, Int16>)TestInt64ToInt16Func);
        }
        private static Int16 TestInt64ToInt16Func([ParamFullRange]Int64 v) {
            return (Int16)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToInt32() {
            this.Test((Func<Int64, Int32>)TestInt64ToInt32Func);
        }
        private static Int32 TestInt64ToInt32Func([ParamFullRange]Int64 v) {
            return (Int32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToUInt16() {
            this.Test((Func<Int64, UInt16>)TestInt64ToUInt16Func);
        }
        private static UInt16 TestInt64ToUInt16Func([ParamFullRange]Int64 v) {
            return (UInt16)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToUInt32() {
            this.Test((Func<Int64, UInt32>)TestInt64ToUInt32Func);
        }
        private static UInt32 TestInt64ToUInt32Func([ParamFullRange]Int64 v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToUInt64() {
            this.Test((Func<Int64, UInt64>)TestInt64ToUInt64Func);
        }
        private static UInt64 TestInt64ToUInt64Func([ParamFullRange]Int64 v) {
            return (UInt64)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToSingle() {
            this.Test((Func<Int64, Single>)TestInt64ToSingleFunc);
        }
        private static Single TestInt64ToSingleFunc([ParamFullRange]Int64 v) {
            return (Single)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestInt64ToDouble() {
            this.Test((Func<Int64, Double>)TestInt64ToDoubleFunc);
        }
        private static Double TestInt64ToDoubleFunc([ParamFullRange]Int64 v) {
            return (Double)v;
        }

        [Test]
        public void TestUInt16ToSByte() {
            this.Test((Func<UInt16, SByte>)TestUInt16ToSByteFunc);
        }
        private static SByte TestUInt16ToSByteFunc([ParamFullRange]UInt16 v) {
            return (SByte)v;
        }

        [Test]
        public void TestUInt16ToByte() {
            this.Test((Func<UInt16, Byte>)TestUInt16ToByteFunc);
        }
        private static Byte TestUInt16ToByteFunc([ParamFullRange]UInt16 v) {
            return (Byte)v;
        }

        [Test]
        public void TestUInt16ToInt16() {
            this.Test((Func<UInt16, Int16>)TestUInt16ToInt16Func);
        }
        private static Int16 TestUInt16ToInt16Func([ParamFullRange]UInt16 v) {
            return (Int16)v;
        }

        [Test]
        public void TestUInt16ToInt32() {
            this.Test((Func<UInt16, Int32>)TestUInt16ToInt32Func);
        }
        private static Int32 TestUInt16ToInt32Func([ParamFullRange]UInt16 v) {
            return (Int32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt16ToInt64() {
            this.Test((Func<UInt16, Int64>)TestUInt16ToInt64Func);
        }
        private static Int64 TestUInt16ToInt64Func([ParamFullRange]UInt16 v) {
            return (Int64)v;
        }

        [Test]
        public void TestUInt16ToUInt32() {
            this.Test((Func<UInt16, UInt32>)TestUInt16ToUInt32Func);
        }
        private static UInt32 TestUInt16ToUInt32Func([ParamFullRange]UInt16 v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt16ToUInt64() {
            this.Test((Func<UInt16, UInt64>)TestUInt16ToUInt64Func);
        }
        private static UInt64 TestUInt16ToUInt64Func([ParamFullRange]UInt16 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestUInt16ToSingle() {
            this.Test((Func<UInt16, Single>)TestUInt16ToSingleFunc);
        }
        private static Single TestUInt16ToSingleFunc([ParamFullRange]UInt16 v) {
            return (Single)v;
        }

        [Test]
        public void TestUInt16ToDouble() {
            this.Test((Func<UInt16, Double>)TestUInt16ToDoubleFunc);
        }
        private static Double TestUInt16ToDoubleFunc([ParamFullRange]UInt16 v) {
            return (Double)v;
        }

        [Test]
        public void TestUInt32ToSByte() {
            this.Test((Func<UInt32, SByte>)TestUInt32ToSByteFunc);
        }
        private static SByte TestUInt32ToSByteFunc([ParamFullRange]UInt32 v) {
            return (SByte)v;
        }

        [Test]
        public void TestUInt32ToByte() {
            this.Test((Func<UInt32, Byte>)TestUInt32ToByteFunc);
        }
        private static Byte TestUInt32ToByteFunc([ParamFullRange]UInt32 v) {
            return (Byte)v;
        }

        [Test]
        public void TestUInt32ToInt16() {
            this.Test((Func<UInt32, Int16>)TestUInt32ToInt16Func);
        }
        private static Int16 TestUInt32ToInt16Func([ParamFullRange]UInt32 v) {
            return (Int16)v;
        }

        [Test]
        public void TestUInt32ToInt32() {
            this.Test((Func<UInt32, Int32>)TestUInt32ToInt32Func);
        }
        private static Int32 TestUInt32ToInt32Func([ParamFullRange]UInt32 v) {
            return (Int32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt32ToInt64() {
            this.Test((Func<UInt32, Int64>)TestUInt32ToInt64Func);
        }
        private static Int64 TestUInt32ToInt64Func([ParamFullRange]UInt32 v) {
            return (Int64)v;
        }

        [Test]
        public void TestUInt32ToUInt16() {
            this.Test((Func<UInt32, UInt16>)TestUInt32ToUInt16Func);
        }
        private static UInt16 TestUInt32ToUInt16Func([ParamFullRange]UInt32 v) {
            return (UInt16)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt32ToUInt64() {
            this.Test((Func<UInt32, UInt64>)TestUInt32ToUInt64Func);
        }
        private static UInt64 TestUInt32ToUInt64Func([ParamFullRange]UInt32 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestUInt32ToSingle() {
            this.Test((Func<UInt32, Single>)TestUInt32ToSingleFunc);
        }
        private static Single TestUInt32ToSingleFunc([ParamFullRange]UInt32 v) {
            return (Single)v;
        }

        [Test]
        public void TestUInt32ToDouble() {
            this.Test((Func<UInt32, Double>)TestUInt32ToDoubleFunc);
        }
        private static Double TestUInt32ToDoubleFunc([ParamFullRange]UInt32 v) {
            return (Double)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToSByte() {
            this.Test((Func<UInt64, SByte>)TestUInt64ToSByteFunc);
        }
        private static SByte TestUInt64ToSByteFunc([ParamFullRange]UInt64 v) {
            return (SByte)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToByte() {
            this.Test((Func<UInt64, Byte>)TestUInt64ToByteFunc);
        }
        private static Byte TestUInt64ToByteFunc([ParamFullRange]UInt64 v) {
            return (Byte)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToInt16() {
            this.Test((Func<UInt64, Int16>)TestUInt64ToInt16Func);
        }
        private static Int16 TestUInt64ToInt16Func([ParamFullRange]UInt64 v) {
            return (Int16)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToInt32() {
            this.Test((Func<UInt64, Int32>)TestUInt64ToInt32Func);
        }
        private static Int32 TestUInt64ToInt32Func([ParamFullRange]UInt64 v) {
            return (Int32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToInt64() {
            this.Test((Func<UInt64, Int64>)TestUInt64ToInt64Func);
        }
        private static Int64 TestUInt64ToInt64Func([ParamFullRange]UInt64 v) {
            return (Int64)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToUInt16() {
            this.Test((Func<UInt64, UInt16>)TestUInt64ToUInt16Func);
        }
        private static UInt16 TestUInt64ToUInt16Func([ParamFullRange]UInt64 v) {
            return (UInt16)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToUInt32() {
            this.Test((Func<UInt64, UInt32>)TestUInt64ToUInt32Func);
        }
        private static UInt32 TestUInt64ToUInt32Func([ParamFullRange]UInt64 v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToSingle() {
            this.Test((Func<UInt64, Single>)TestUInt64ToSingleFunc);
        }
        private static Single TestUInt64ToSingleFunc([ParamFullRange]UInt64 v) {
            return (Single)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestUInt64ToDouble() {
            this.Test((Func<UInt64, Double>)TestUInt64ToDoubleFunc);
        }
        private static Double TestUInt64ToDoubleFunc([ParamFullRange]UInt64 v) {
            return (Double)v;
        }

        [Test]
        public void TestSingleToSByte() {
            this.Test((Func<Single, SByte>)TestSingleToSByteFunc);
        }
        private static SByte TestSingleToSByteFunc([ParamFullRange]Single v) {
            return (SByte)v;
        }

        [Test]
        public void TestSingleToByte() {
            this.Test((Func<Single, Byte>)TestSingleToByteFunc);
        }
        private static Byte TestSingleToByteFunc([ParamFullRange]Single v) {
            return (Byte)v;
        }

        [Test]
        public void TestSingleToInt16() {
            this.Test((Func<Single, Int16>)TestSingleToInt16Func);
        }
        private static Int16 TestSingleToInt16Func([ParamFullRange]Single v) {
            return (Int16)v;
        }

        [Test, Ignore("Undefined values differ")]
        public void TestSingleToInt32() {
            this.Test((Func<Single, Int32>)TestSingleToInt32Func);
        }
        private static Int32 TestSingleToInt32Func([ParamFullRange]Single v) {
            return (Int32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestSingleToInt64() {
            this.Test((Func<Single, Int64>)TestSingleToInt64Func);
        }
        private static Int64 TestSingleToInt64Func([ParamFullRange]Single v) {
            return (Int64)v;
        }

        [Test]
        public void TestSingleToUInt16() {
            this.Test((Func<Single, UInt16>)TestSingleToUInt16Func);
        }
        private static UInt16 TestSingleToUInt16Func([ParamFullRange]Single v) {
            return (UInt16)v;
        }

        [Test]
        public void TestSingleToUInt32() {
            this.Test((Func<Single, UInt32>)TestSingleToUInt32Func);
        }
        private static UInt32 TestSingleToUInt32Func([ParamFullRange]Single v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestSingleToUInt64() {
            this.Test((Func<Single, UInt64>)TestSingleToUInt64Func);
        }
        private static UInt64 TestSingleToUInt64Func([ParamFullRange]Single v) {
            return (UInt64)v;
        }

        [Test, Ignore("NaN fails")]
        public void TestSingleToDouble() {
            this.Test((Func<Single, Double>)TestSingleToDoubleFunc);
        }
        private static Double TestSingleToDoubleFunc([ParamFullRange]Single v) {
            return (Double)v;
        }

        [Test]
        public void TestDoubleToSByte() {
            this.Test((Func<Double, SByte>)TestDoubleToSByteFunc);
        }
        private static SByte TestDoubleToSByteFunc([ParamFullRange]Double v) {
            return (SByte)v;
        }

        [Test]
        public void TestDoubleToByte() {
            this.Test((Func<Double, Byte>)TestDoubleToByteFunc);
        }
        private static Byte TestDoubleToByteFunc([ParamFullRange]Double v) {
            return (Byte)v;
        }

        [Test]
        public void TestDoubleToInt16() {
            this.Test((Func<Double, Int16>)TestDoubleToInt16Func);
        }
        private static Int16 TestDoubleToInt16Func([ParamFullRange]Double v) {
            return (Int16)v;
        }

        [Test, Ignore("Undefined values differ")]
        public void TestDoubleToInt32() {
            this.Test((Func<Double, Int32>)TestDoubleToInt32Func);
        }
        private static Int32 TestDoubleToInt32Func([ParamFullRange]Double v) {
            return (Int32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestDoubleToInt64() {
            this.Test((Func<Double, Int64>)TestDoubleToInt64Func);
        }
        private static Int64 TestDoubleToInt64Func([ParamFullRange]Double v) {
            return (Int64)v;
        }

        [Test]
        public void TestDoubleToUInt16() {
            this.Test((Func<Double, UInt16>)TestDoubleToUInt16Func);
        }
        private static UInt16 TestDoubleToUInt16Func([ParamFullRange]Double v) {
            return (UInt16)v;
        }

        [Test]
        public void TestDoubleToUInt32() {
            this.Test((Func<Double, UInt32>)TestDoubleToUInt32Func);
        }
        private static UInt32 TestDoubleToUInt32Func([ParamFullRange]Double v) {
            return (UInt32)v;
        }

        [Test, Ignore("64-bit values not yet properly supported")]
        public void TestDoubleToUInt64() {
            this.Test((Func<Double, UInt64>)TestDoubleToUInt64Func);
        }
        private static UInt64 TestDoubleToUInt64Func([ParamFullRange]Double v) {
            return (UInt64)v;
        }

        [Test, Ignore("NaN fails")]
        public void TestDoubleToSingle() {
            this.Test((Func<Double, Single>)TestDoubleToSingleFunc);
        }
        private static Single TestDoubleToSingleFunc([ParamFullRange]Double v) {
            return (Single)v;
        }

    }

}
