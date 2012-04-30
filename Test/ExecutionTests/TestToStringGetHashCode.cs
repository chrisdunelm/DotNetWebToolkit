using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestToStringGetHashCode : ExecutionTestBase {

        [Test]
        public void TestStringGetHashCode() {
            Func<string, string, bool> f = (a, b) => {
                var ah1 = a.GetHashCode();
                var ah2 = a.GetHashCode();
                var bh = b.GetHashCode();
                return ah1 == ah2 && ah1 != bh;
            };
            this.Test(f);
        }

        [Test]
        public void TestStringToString() {
            Func<string, string> f = a => a.ToString();
            this.Test(f);
        }

        [Test]
        public void TestBooleanGetHashCode() {
            Func<int> f = () => true.GetHashCode() + false.GetHashCode();
            this.Test(f);
        }

        [Test]
        public void TestBooleanToString() {
            Func<string> f = () => true.ToString() + false.ToString();
            this.Test(f);
        }

        [Test]
        public void TestCharGetHashCode() {
            Func<char, int> f = a => a.GetHashCode();
            this.Test(f);
        }

        [Test]
        public void TestCharToString() {
            Func<char, string> f = a => a.ToString();
            this.Test(f);
        }

        [Test]
        public void TestInt32GetHashCode() {
            Func<int, int> f = a => a.GetHashCode();
            this.Test(f);
        }

        [Test]
        public void TestInt32ToString() {
            this.Test((Func<int, string>)TestInt32ToStringFunc);
        }
        private static string TestInt32ToStringFunc([ParamFullRange]int a) {
            return a.ToString();
        }

        [Test]
        public void TestInt32ToStringFormatGeneral() {
            this.Test((Func<int, string>)TestInt32ToStringFormatGeneralFunc);
        }
        private static string TestInt32ToStringFormatGeneralFunc([ParamFullRange]int a) {
            return string.Format("{0:g}:{0:G}:{0:g0}:{0:g1}:{0:G2}:{0:g3}:{0:G4}:{0:g5}:{0:G6}:{0:g7}:{0:G8}:{0:g9}:{0:G10}:{0:g11}:{0:G12}", a);
        }

        [Test]
        public void TestInt32ToStringFormatDecimal() {
            this.Test((Func<int, string>)TestInt32ToStringFormatDecimalFunc);
        }
        private static string TestInt32ToStringFormatDecimalFunc([ParamFullRange]int a) {
            return string.Format("{0:d}:{0:D}:{0:d0}:{0:d1}:{0:D2}:{0:d3}:{0:D4}:{0:d5}:{0:D6}:{0:d7}:{0:D8}:{0:d9}:{0:D10}:{0:d11}:{0:D12}", a);
        }

        [Test]
        public void TestInt32ToStringFormatHex() {
            this.Test((Func<int, string>)TestInt32ToStringFormatHexFunc);
        }
        private static string TestInt32ToStringFormatHexFunc([ParamFullRange]int a) {
            return string.Format("{0:x}:{0:X}:{0:x0}:{0:x1}:{0:X2}:{0:x3}:{0:X4}:{0:x5}:{0:X6}:{0:x7}:{0:X8}:{0:x9}:{0:X10}:{0:x11}:{0:X12}", a);
        }

        [Test]
        public void TestInt32ToStringFormatNumber() {
            this.Test((Func<int, string>)TestInt32ToStringFormatNumberFunc);
        }
        private static string TestInt32ToStringFormatNumberFunc([ParamFullRange]int a) {
            return string.Format("{0:n}:{0:N}:{0:n0}:{0:n1}:{0:N2}:{0:n3}:{0:N4}:{0:n5}:{0:N6}:{0:n7}:{0:N8}:{0:n9}:{0:N10}:{0:n11}:{0:N12}", a);
        }

        [Test]
        public void TestInt64GetHashCode() {
            this.Test((Func<Int64, int>)TestInt64GetHashCodeFunc);
        }
        private static int TestInt64GetHashCodeFunc([ParamFullRange]Int64 a) {
            return a.GetHashCode();
        }

        [Test]
        public void TestInt64ToString() {
            this.Test((Func<Int64, string>)TestInt64ToStringFunc);
        }
        private static string TestInt64ToStringFunc([ParamFullRange]Int64 a) {
            return a.ToString();
        }

        [Test]
        public void TestInt64ToStringFormatGeneral() {
            this.Test((Func<Int64, string>)TestInt64ToStringFormatGeneralFunc);
        }
        private static string TestInt64ToStringFormatGeneralFunc([ParamFullRange]Int64 a) {
            return string.Format("{0:g}:{0:G}:{0:g0}:{0:g1}:{0:G2}:{0:g3}:{0:G4}:{0:g5}:{0:G6}:{0:g7}:{0:G8}:{0:g9}:{0:G10}:{0:g11}:{0:G12}:{0:g13}:{0:G14}:{0:g15}:{0:G16}:{0:g17}:{0:G18}:{0:g19}:{0:G20}:{0:g21}:{0:G22}:{0:g23}:{0:G24}", a);
        }

        [Test]
        public void TestInt64ToStringFormatDecimal() {
            this.Test((Func<Int64, string>)TestInt64ToStringFormatDecimalFunc);
        }
        private static string TestInt64ToStringFormatDecimalFunc([ParamFullRange]Int64 a) {
            return string.Format("{0:d}:{0:D}:{0:d0}:{0:d1}:{0:D2}:{0:d3}:{0:D4}:{0:d5}:{0:D6}:{0:d7}:{0:D8}:{0:d9}:{0:D10}:{0:d11}:{0:D12}:{0:d13}:{0:D14}:{0:d15}:{0:D16}:{0:d17}:{0:D18}:{0:d19}:{0:D20}:{0:d21}:{0:D22}:{0:d23}:{0:D24}", a);
        }

        [Test]
        public void TestInt64ToStringFormatHex() {
            this.Test((Func<Int64, string>)TestInt64ToStringFormatHexFunc);
        }
        private static string TestInt64ToStringFormatHexFunc([ParamFullRange]Int64 a) {
            return string.Format("{0:x}:{0:X}:{0:x0}:{0:x1}:{0:X2}:{0:x3}:{0:X4}:{0:x5}:{0:X6}:{0:x7}:{0:X8}:{0:x9}:{0:X10}:{0:x11}:{0:X12}:{0:x13}:{0:X14}:{0:x15}:{0:X16}:{0:x17}:{0:X18}:{0:x19}:{0:X20}:{0:x21}:{0:X22}:{0:x23}:{0:X24}", a);
        }

        [Test]
        public void TestInt64ToStringFormatNumber() {
            this.Test((Func<Int64, string>)TestInt64ToStringFormatNumberFunc);
        }
        private static string TestInt64ToStringFormatNumberFunc([ParamFullRange]Int64 a) {
            return string.Format("{0:n}:{0:N}:{0:n0}:{0:n1}:{0:N2}:{0:n3}:{0:N4}:{0:n5}:{0:N6}:{0:n7}:{0:N8}:{0:n9}:{0:N10}:{0:n11}:{0:N12}:{0:n13}:{0:N14}:{0:n15}:{0:N16}:{0:n17}:{0:N18}:{0:n19}:{0:N20}:{0:n21}:{0:N22}:{0:n23}:{0:N24}", a);
        }

        [Test]
        public void TestDoubleGetHashCode() {
            this.Test((Func<double, double, bool>)TestDoubleGetHashCode);
        }
        private static bool TestDoubleGetHashCode([ParamFullRange]double a, [ParamFullRange]double b) {
            var ah = a.GetHashCode();
            var bh = b.GetHashCode();
            return a == b ? ah == bh : ah != bh;
        }

        [Test]
        public void TestDoubleToString() {
            this.Test((Func<double, string>)TestDoubleToString);
        }
        private static string TestDoubleToString([ParamFullRange]double a) {
            return a.ToString();
        }

        [Test]
        public void TestDoubleToStringFormatGeneral() {
            this.Test((Func<double, string>)TestDoubleToStringFormatGeneralFunc);
        }
        private static string TestDoubleToStringFormatGeneralFunc([ParamFullRange]double a) {
            return string.Format("{0:g}:{0:G}:{0:g0}:{0:g1}:{0:G2}:{0:g3}:{0:G4}:{0:g5}:{0:G6}:{0:g7}:{0:G8}:{0:g9}:{0:G10}:{0:g11}:{0:G12}", a);
        }

        [Test]
        public void TestDoubleToStringFormatNumber() {
            this.Test((Func<double, string>)TestDoubleToStringFormatNumberFunc);
        }
        private static string TestDoubleToStringFormatNumberFunc([ParamFullRange]double a) {
            return string.Format("{0:n}:{0:N}:{0:n0}:{0:n1}:{0:N2}:{0:n3}:{0:N4}:{0:n5}:{0:N6}:{0:n7}:{0:N8}:{0:n9}:{0:N10}:{0:n11}:{0:N12}", a);
        }

        [Test]
        public void TestDoubleToStringFormatFixed() {
            this.Test((Func<double, string>)TestDoubleToStringFormatFixedFunc);
        }
        private static string TestDoubleToStringFormatFixedFunc([ParamFullRange]double a) {
            return string.Format("{0:f}:{0:F}:{0:f0}:{0:f1}:{0:F2}:{0:f3}:{0:F4}:{0:f5}:{0:F6}:{0:f7}:{0:F8}:{0:f9}:{0:F10}:{0:f11}:{0:F12}", a);
        }

    }

}
