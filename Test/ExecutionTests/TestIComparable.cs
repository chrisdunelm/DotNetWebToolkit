using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestIComparable : ExecutionTestBase {

        [Test]
        public void TestInt8() {
            Func<sbyte, sbyte, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestInt16() {
            Func<Int16, Int16, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestInt32() {
            Func<Int32, Int32, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestInt64() {
            Func<Int64, Int64, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestUInt8() {
            Func<byte, byte, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestUInt16() {
            Func<UInt16, UInt16, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestUInt32() {
            Func<UInt32, UInt32, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestUInt64() {
            Func<UInt64, UInt64, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestSingle() {
            Func<Single, Single, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestDouble() {
            Func<Double, Double, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestBoolean() {
            Func<bool, bool, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestChar() {
            Func<char, char, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

        [Test]
        public void TestString() {
            Func<string, string, int> f = (a, b) => a.CompareTo(b);
            this.Test(f);
        }

    }

}
