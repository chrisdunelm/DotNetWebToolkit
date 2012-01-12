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
        public void TestIntegerDiv1() {
            Func<int, int, int> f = (a, b) => a / b;
            this.Test(f);
        }

        [Test]
        public void TestIntegerDiv2() {
            Func<int, int, int> f = (a, b) => (-a) / b;
            this.Test(f);
        }

        [Test]
        public void TestIntegerDiv3() {
            Func<int, int, int> f = (a, b) => a / (-b);
            this.Test(f);
        }

        [Test]
        public void TestIntegerDiv4() {
            Func<int, int, int> f = (a, b) => (-a) / (-b);
            this.Test(f);
        }

        [Test]
        public void TestDoubleDiv() {
            Func<double, double, double> f = (a, b) => a / b;
            this.Test(f);
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
