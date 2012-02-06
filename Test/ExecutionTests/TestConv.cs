using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    using Int8 = SByte;
    using UInt8 = Byte;

    [TestFixture]
    public class TestConv : ExecutionTestBase {

        private const Int32 Int32Min = Int32.MinValue + 100;
        private const Int32 Int32Max = Int32.MaxValue - 100;
        private const UInt32 UInt32Min = 0;
        private const UInt32 UInt32Max = UInt32.MaxValue / 2 - 100;
        private const Int64 Int64Min = Int64.MinValue + 100;
        private const Int64 Int64Max = Int64.MaxValue - 100;
        private const UInt64 UInt64Min = 0;
        private const UInt64 UInt64Max = UInt64.MaxValue / 2 - 100;

        [Test]
        public void TestInt8ToInt16() {
            this.Test((Func<Int8, Int16>)TestInt8ToInt16Func);
        }
        private static Int16 TestInt8ToInt16Func([ParamFullRange]Int8 v) {
            return (Int16)v;
        }

        [Test]
        public void TestInt8ToInt32() {
            this.Test((Func<Int8, Int32>)TestInt8ToInt32Func);
        }
        private static Int32 TestInt8ToInt32Func([ParamFullRange]Int8 v) {
            return (Int32)v;
        }

        [Test]
        public void TestInt8ToInt64() {
            this.Test((Func<Int8, Int64>)TestInt8ToInt64Func);
        }
        private static Int64 TestInt8ToInt64Func([ParamFullRange]Int8 v) {
            return (Int64)v;
        }

        [Test]
        public void TestInt8ToUInt8() {
            this.Test((Func<Int8, UInt8>)TestInt8ToUInt8Func);
        }
        private static UInt8 TestInt8ToUInt8Func([ParamFullRange]Int8 v) {
            return (UInt8)v;
        }

        [Test]
        public void TestInt8ToUInt16() {
            this.Test((Func<Int8, UInt16>)TestInt8ToUInt16Func);
        }
        private static UInt16 TestInt8ToUInt16Func([ParamFullRange]Int8 v) {
            return (UInt16)v;
        }

        [Test]
        public void TestInt8ToUInt32() {
            this.Test((Func<Int8, UInt32>)TestInt8ToUInt32Func);
        }
        private static UInt32 TestInt8ToUInt32Func([ParamFullRange]Int8 v) {
            return (UInt32)v;
        }

        [Test]
        public void TestInt8ToUInt64() {
            this.Test((Func<Int8, UInt64>)TestInt8ToUInt64Func);
        }
        private static UInt64 TestInt8ToUInt64Func([ParamFullRange]Int8 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestInt8ToSingle() {
            this.Test((Func<Int8, Single>)TestInt8ToSingleFunc);
        }
        private static Single TestInt8ToSingleFunc([ParamFullRange]Int8 v) {
            return (Single)v;
        }

        [Test]
        public void TestInt8ToDouble() {
            this.Test((Func<Int8, Double>)TestInt8ToDoubleFunc);
        }
        private static Double TestInt8ToDoubleFunc([ParamFullRange]Int8 v) {
            return (Double)v;
        }

        [Test]
        public void TestUInt8ToInt8() {
            this.Test((Func<UInt8, Int8>)TestUInt8ToInt8Func);
        }
        private static Int8 TestUInt8ToInt8Func([ParamFullRange]UInt8 v) {
            return (Int8)v;
        }

        [Test]
        public void TestUInt8ToInt16() {
            this.Test((Func<UInt8, Int16>)TestUInt8ToInt16Func);
        }
        private static Int16 TestUInt8ToInt16Func([ParamFullRange]UInt8 v) {
            return (Int16)v;
        }

        [Test]
        public void TestUInt8ToInt32() {
            this.Test((Func<UInt8, Int32>)TestUInt8ToInt32Func);
        }
        private static Int32 TestUInt8ToInt32Func([ParamFullRange]UInt8 v) {
            return (Int32)v;
        }

        [Test]
        public void TestUInt8ToInt64() {
            this.Test((Func<UInt8, Int64>)TestUInt8ToInt64Func);
        }
        private static Int64 TestUInt8ToInt64Func([ParamFullRange]UInt8 v) {
            return (Int64)v;
        }

        [Test]
        public void TestUInt8ToUInt16() {
            this.Test((Func<UInt8, UInt16>)TestUInt8ToUInt16Func);
        }
        private static UInt16 TestUInt8ToUInt16Func([ParamFullRange]UInt8 v) {
            return (UInt16)v;
        }

        [Test]
        public void TestUInt8ToUInt32() {
            this.Test((Func<UInt8, UInt32>)TestUInt8ToUInt32Func);
        }
        private static UInt32 TestUInt8ToUInt32Func([ParamFullRange]UInt8 v) {
            return (UInt32)v;
        }

        [Test]
        public void TestUInt8ToUInt64() {
            this.Test((Func<UInt8, UInt64>)TestUInt8ToUInt64Func);
        }
        private static UInt64 TestUInt8ToUInt64Func([ParamFullRange]UInt8 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestUInt8ToSingle() {
            this.Test((Func<UInt8, Single>)TestUInt8ToSingleFunc);
        }
        private static Single TestUInt8ToSingleFunc([ParamFullRange]UInt8 v) {
            return (Single)v;
        }

        [Test]
        public void TestUInt8ToDouble() {
            this.Test((Func<UInt8, Double>)TestUInt8ToDoubleFunc);
        }
        private static Double TestUInt8ToDoubleFunc([ParamFullRange]UInt8 v) {
            return (Double)v;
        }

        [Test]
        public void TestInt16ToInt8() {
            this.Test((Func<Int16, Int8>)TestInt16ToInt8Func);
        }
        private static Int8 TestInt16ToInt8Func([ParamFullRange]Int16 v) {
            return (Int8)v;
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
        public void TestInt16ToUInt8() {
            this.Test((Func<Int16, UInt8>)TestInt16ToUInt8Func);
        }
        private static UInt8 TestInt16ToUInt8Func([ParamFullRange]Int16 v) {
            return (UInt8)v;
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

        [Test]
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
        public void TestInt32ToInt8() {
            this.Test((Func<Int32, Int8>)TestInt32ToInt8Func);
        }
        private static Int8 TestInt32ToInt8Func([ParamFullRange]Int32 v) {
            return (Int8)v;
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
        public void TestInt32ToUInt8() {
            this.Test((Func<Int32, UInt8>)TestInt32ToUInt8Func);
        }
        private static UInt8 TestInt32ToUInt8Func([ParamFullRange]Int32 v) {
            return (UInt8)v;
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

        [Test]
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

        [Test]
        public void TestInt64ToInt8() {
            this.Test((Func<Int64, Int8>)TestInt64ToInt8Func);
        }
        private static Int8 TestInt64ToInt8Func([ParamFullRange]Int64 v) {
            return (Int8)v;
        }

        [Test]
        public void TestInt64ToInt16() {
            this.Test((Func<Int64, Int16>)TestInt64ToInt16Func);
        }
        private static Int16 TestInt64ToInt16Func([ParamFullRange]Int64 v) {
            return (Int16)v;
        }

        [Test]
        public void TestInt64ToInt32() {
            this.Test((Func<Int64, Int32>)TestInt64ToInt32Func);
        }
        private static Int32 TestInt64ToInt32Func([ParamFullRange]Int64 v) {
            return (Int32)v;
        }

        [Test]
        public void TestInt64ToUInt8() {
            this.Test((Func<Int64, UInt8>)TestInt64ToUInt8Func);
        }
        private static UInt8 TestInt64ToUInt8Func([ParamFullRange]Int64 v) {
            return (UInt8)v;
        }

        [Test]
        public void TestInt64ToUInt16() {
            this.Test((Func<Int64, UInt16>)TestInt64ToUInt16Func);
        }
        private static UInt16 TestInt64ToUInt16Func([ParamFullRange]Int64 v) {
            return (UInt16)v;
        }

        [Test]
        public void TestInt64ToUInt32() {
            this.Test((Func<Int64, UInt32>)TestInt64ToUInt32Func);
        }
        private static UInt32 TestInt64ToUInt32Func([ParamFullRange]Int64 v) {
            return (UInt32)v;
        }

        [Test]
        public void TestInt64ToUInt64() {
            this.Test((Func<Int64, UInt64>)TestInt64ToUInt64Func);
        }
        private static UInt64 TestInt64ToUInt64Func([ParamFullRange]Int64 v) {
            return (UInt64)v;
        }

        [Test]
        public void TestInt64ToSingle() {
            this.Test((Func<Int64, Single>)TestInt64ToSingleFunc);
        }
        [WithinUlps(1)]
        private static Single TestInt64ToSingleFunc([ParamFullRange]Int64 v) {
            return (Single)v;
        }

        [Test]
        public void TestInt64ToDouble() {
            this.Test((Func<Int64, Double>)TestInt64ToDoubleFunc);
        }
        [WithinUlps(2)]
        private static Double TestInt64ToDoubleFunc([ParamFullRange]Int64 v) {
            return (Double)v;
        }

        [Test]
        public void TestUInt16ToInt8() {
            this.Test((Func<UInt16, Int8>)TestUInt16ToInt8Func);
        }
        private static Int8 TestUInt16ToInt8Func([ParamFullRange]UInt16 v) {
            return (Int8)v;
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

        [Test]
        public void TestUInt16ToInt64() {
            this.Test((Func<UInt16, Int64>)TestUInt16ToInt64Func);
        }
        private static Int64 TestUInt16ToInt64Func([ParamFullRange]UInt16 v) {
            return (Int64)v;
        }

        [Test]
        public void TestUInt16ToUInt8() {
            this.Test((Func<UInt16, UInt8>)TestUInt16ToUInt8Func);
        }
        private static UInt8 TestUInt16ToUInt8Func([ParamFullRange]UInt16 v) {
            return (UInt8)v;
        }

        [Test]
        public void TestUInt16ToUInt32() {
            this.Test((Func<UInt16, UInt32>)TestUInt16ToUInt32Func);
        }
        private static UInt32 TestUInt16ToUInt32Func([ParamFullRange]UInt16 v) {
            return (UInt32)v;
        }

        [Test]
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
        public void TestUInt32ToInt8() {
            this.Test((Func<UInt32, Int8>)TestUInt32ToInt8Func);
        }
        private static Int8 TestUInt32ToInt8Func([ParamFullRange]UInt32 v) {
            return (Int8)v;
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

        [Test]
        public void TestUInt32ToInt64() {
            this.Test((Func<UInt32, Int64>)TestUInt32ToInt64Func);
        }
        private static Int64 TestUInt32ToInt64Func([ParamFullRange]UInt32 v) {
            return (Int64)v;
        }

        [Test]
        public void TestUInt32ToUInt8() {
            this.Test((Func<UInt32, UInt8>)TestUInt32ToUInt8Func);
        }
        private static UInt8 TestUInt32ToUInt8Func([ParamFullRange]UInt32 v) {
            return (UInt8)v;
        }

        [Test]
        public void TestUInt32ToUInt16() {
            this.Test((Func<UInt32, UInt16>)TestUInt32ToUInt16Func);
        }
        private static UInt16 TestUInt32ToUInt16Func([ParamFullRange]UInt32 v) {
            return (UInt16)v;
        }

        [Test]
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

        [Test]
        public void TestUInt64ToInt8() {
            this.Test((Func<UInt64, Int8>)TestUInt64ToInt8Func);
        }
        private static Int8 TestUInt64ToInt8Func([ParamFullRange]UInt64 v) {
            return (Int8)v;
        }

        [Test]
        public void TestUInt64ToInt16() {
            this.Test((Func<UInt64, Int16>)TestUInt64ToInt16Func);
        }
        private static Int16 TestUInt64ToInt16Func([ParamFullRange]UInt64 v) {
            return (Int16)v;
        }

        [Test]
        public void TestUInt64ToInt32() {
            this.Test((Func<UInt64, Int32>)TestUInt64ToInt32Func);
        }
        private static Int32 TestUInt64ToInt32Func([ParamFullRange]UInt64 v) {
            return (Int32)v;
        }

        [Test]
        public void TestUInt64ToInt64() {
            this.Test((Func<UInt64, Int64>)TestUInt64ToInt64Func);
        }
        private static Int64 TestUInt64ToInt64Func([ParamFullRange]UInt64 v) {
            return (Int64)v;
        }

        [Test]
        public void TestUInt64ToUInt8() {
            this.Test((Func<UInt64, UInt8>)TestUInt64ToUInt8Func);
        }
        private static UInt8 TestUInt64ToUInt8Func([ParamFullRange]UInt64 v) {
            return (UInt8)v;
        }

        [Test]
        public void TestUInt64ToUInt16() {
            this.Test((Func<UInt64, UInt16>)TestUInt64ToUInt16Func);
        }
        private static UInt16 TestUInt64ToUInt16Func([ParamFullRange]UInt64 v) {
            return (UInt16)v;
        }

        [Test]
        public void TestUInt64ToUInt32() {
            this.Test((Func<UInt64, UInt32>)TestUInt64ToUInt32Func);
        }
        private static UInt32 TestUInt64ToUInt32Func([ParamFullRange]UInt64 v) {
            return (UInt32)v;
        }

        [Test]
        public void TestUInt64ToSingle() {
            this.Test((Func<UInt64, Single>)TestUInt64ToSingleFunc);
        }
        [WithinUlps(1)]
        private static Single TestUInt64ToSingleFunc([ParamFullRange]UInt64 v) {
            return (Single)v;
        }

        [Test]
        public void TestUInt64ToDouble() {
            this.Test((Func<UInt64, Double>)TestUInt64ToDoubleFunc);
        }
        [WithinUlps(2)]
        private static Double TestUInt64ToDoubleFunc([ParamFullRange]UInt64 v) {
            return (Double)v;
        }

        [Test]
        public void TestSingleToInt8() {
            this.Test((Func<Single, Int8>)TestSingleToInt8Func);
        }
        private static Int8 TestSingleToInt8Func([ParamFullRange]Single v) {
            return (Int8)v;
        }

        [Test]
        public void TestSingleToInt16() {
            this.Test((Func<Single, Int16>)TestSingleToInt16Func);
        }
        private static Int16 TestSingleToInt16Func([ParamFullRange]Single v) {
            return (Int16)v;
        }

        [Test]
        public void TestSingleToInt32() {
            this.Test((Func<Single, Int32>)TestSingleToInt32Func);
        }
        [WithinPercent(0.00001)]
        private static Int32 TestSingleToInt32Func([ParamFullRange(Int32Min, Int32Max)]Single v) {
            return (Int32)v;
        }

        [Test]
        public void TestSingleToInt64() {
            this.Test((Func<Single, Int64>)TestSingleToInt64Func);
        }
        [WithinPercent(0.00001)]
        private static Int64 TestSingleToInt64Func([ParamFullRange(Int64Min, Int64Max)]Single v) {
            return (Int64)v;
        }

        [Test]
        public void TestSingleToUInt8() {
            this.Test((Func<Single, UInt8>)TestSingleToUInt8Func);
        }
        private static UInt8 TestSingleToUInt8Func([ParamFullRange]Single v) {
            return (UInt8)v;
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
        [WithinPercent(0.00001)]
        private static UInt32 TestSingleToUInt32Func([ParamFullRange(UInt32Min, UInt32Max)]Single v) {
            return (UInt32)v;
        }

        [Test]
        public void TestSingleToUInt64() {
            this.Test((Func<Single, UInt64>)TestSingleToUInt64Func);
        }
        [WithinPercent(0.00001)]
        private static UInt64 TestSingleToUInt64Func([ParamFullRange(UInt64Min, UInt64Max)]Single v) {
            return (UInt64)v;
        }

        [Test]
        public void TestSingleToDouble() {
            this.Test((Func<Single, Double>)TestSingleToDoubleFunc);
        }
        [WithinPercent(0.00001)]
        private static Double TestSingleToDoubleFunc([ParamFullRange(float.MinValue, float.MaxValue)]Single v) {
            return (Double)v;
        }

        [Test]
        public void TestDoubleToInt8() {
            this.Test((Func<Double, Int8>)TestDoubleToInt8Func);
        }
        private static Int8 TestDoubleToInt8Func([ParamFullRange]Double v) {
            return (Int8)v;
        }

        [Test]
        public void TestDoubleToInt16() {
            this.Test((Func<Double, Int16>)TestDoubleToInt16Func);
        }
        private static Int16 TestDoubleToInt16Func([ParamFullRange]Double v) {
            return (Int16)v;
        }

        [Test]
        public void TestDoubleToInt32() {
            this.Test((Func<Double, Int32>)TestDoubleToInt32Func);
        }
        [WithinPercent(0.00001)]
        private static Int32 TestDoubleToInt32Func([ParamFullRange(Int32Min, Int32Max)]Double v) {
            return (Int32)v;
        }

        [Test]
        public void TestDoubleToInt64() {
            this.Test((Func<Double, Int64>)TestDoubleToInt64Func);
        }
        [WithinPercent(0.00001)]
        private static Int64 TestDoubleToInt64Func([ParamFullRange(Int64Min, Int64Max)]Double v) {
            return (Int64)v;
        }

        [Test]
        public void TestDoubleToUInt8() {
            this.Test((Func<Double, UInt8>)TestDoubleToUInt8Func);
        }
        private static UInt8 TestDoubleToUInt8Func([ParamFullRange]Double v) {
            return (UInt8)v;
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

        [Test]
        public void TestDoubleToUInt64() {
            this.Test((Func<Double, UInt64>)TestDoubleToUInt64Func);
        }
        [WithinPercent(0.00001)]
        private static UInt64 TestDoubleToUInt64Func([ParamFullRange(UInt64Min, UInt64Max)]Double v) {
            return (UInt64)v;
        }

        [Test]
        public void TestDoubleToSingle() {
            this.Test((Func<Double, Single>)TestDoubleToSingleFunc);
        }
        private static Single TestDoubleToSingleFunc([ParamFullRange(float.MinValue, float.MaxValue)]Double v) {
            return (Single)v;
        }

    }

}
