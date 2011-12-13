﻿using System;
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

        [Test, Ignore("Integer division not yet supported - currently performs floating point division")]
        public void TestDiv() {
            Func<int, int, int> f = (a, b) => a / b;
            this.Test(f);
        }

        [Test]
        public void TestDiv2() {
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
