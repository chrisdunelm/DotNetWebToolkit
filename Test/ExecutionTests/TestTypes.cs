using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestTypes : ExecutionTestBase {

        [Test]
        public void TestDouble() {
            Func<string> f = () => {
                var d = 0.1;
                if (d < 0.0) {
                    return "<";
                } else {
                    return ">";
                }
            };
            this.Test(f);
        }

        static UInt16 GetUInt16Max() {
            return UInt16.MaxValue;
        }
        [Test]
        public void TestUInt16EqLargeLiteral() {
            Func<bool> f = () => GetUInt16Max() == 0xffffU;
            this.Test(f, true);
        }
        [Test]
        public void TestUInt16NeqLargeLiteral() {
            Func<bool> f = () => GetUInt16Max() != 0xffffU;
            this.Test(f, false);
        }

        static UInt32 GetUInt32Max() {
            return UInt32.MaxValue;
        }
        [Test]
        public void TestUInt32EqLargeLiteral() {
            Func<bool> f = () => GetUInt32Max() == 0xffffffffU;
            this.Test(f, true);
        }
        [Test]
        public void TestUInt32NeqLargeLiteral() {
            Func<bool> f = () => GetUInt32Max() != 0xffffffffU;
            this.Test(f, false);
        }

        static UInt32 GetUint32MaxM1() {
            return UInt32.MaxValue - 1;
        }

        [Test]
        public void TestUInt32LessThanLiteral() {
            Func<bool> f = () => GetUint32MaxM1() < 0xffffffffU;
            this.Test(f, true);
        }
        [Test]
        public void TestUInt32NotLessThanLiteral() {
            Func<bool> f = () => GetUint32MaxM1() < 0xfffffffeU;
            this.Test(f, false);
        }

        [Test]
        public void TestUInt32LessThanOrEqualLiteral() {
            Func<bool> f = () => GetUint32MaxM1() <= 0xfffffffeU;
            this.Test(f, true);
        }
        [Test]
        public void TestUInt32NotLessThanOrEqualLiteral() {
            Func<bool> f = () => GetUint32MaxM1() <= 0xfffffffdU;
            this.Test(f, false);
        }

        [Test]
        public void TestUInt32GreaterThanLiteral() {
            Func<bool> f = () => GetUint32MaxM1() > 0xfffffffdU;
            this.Test(f, true);
        }
        [Test]
        public void TestUInt32NotGreaterThanLiteral() {
            Func<bool> f = () => GetUint32MaxM1() > 0xfffffffeU;
            this.Test(f, false);
        }

        [Test]
        public void TestUInt32GreaterThanOrEqualLiteral() {
            Func<bool> f = () => GetUint32MaxM1() >= 0xfffffffeU;
            this.Test(f, true);
        }
        [Test]
        public void TestUInt32NotGreaterThanOrEqualLiteral() {
            Func<bool> f = () => GetUint32MaxM1() >= 0xffffffffU;
            this.Test(f, false);
        }

    }

}
