using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestMethodCalls : ExecutionTestBase {

        class C0 {
            public int i = 0;
            public static int S2(int a, int b) { return a + b; }
            public static int S3(int a, int b, int c) { return a + b + c; }
            public static int S4(int a, int b, int c, int d) { return a + b + c + d; }
            public static int S5(int a, int b, int c, int d, int e) { return a + b + c + d + e; }
            public int I2(int a, int b) { return a + b; }
            public int I3(int a, int b, int c) { return a + b + c; }
            public int I4(int a, int b, int c, int d) { return a + b + c + d; }
            public int I5(int a, int b, int c, int d, int e) { return a + b + c + d + e; }
            public int T2(int a, int b) { return this.i + a + b; }
            public int T3(int a, int b, int c) { return this.i + a + b + c; }
            public int T4(int a, int b, int c, int d) { return this.i + a + b + c + d; }
            public int T5(int a, int b, int c, int d, int e) { return this.i + a + b + c + d + e; }
        }

        [Test]
        public void TestS2() {
            Func<int, int, int> z = (a, b) => C0.S2(a, b);
            this.Test(z);
        }

        [Test]
        public void TestS3() {
            Func<int, int, int, int> z = (a, b, c) => C0.S3(a, b, c);
            this.Test(z);
        }

        [Test]
        public void TestS4() {
            Func<int, int, int, int, int> z = (a, b, c, d) => C0.S4(a, b, c, d);
            this.Test(z);
        }

        [Test]
        public void TestS5() {
            Func<int, int, int, int, int, int> z = (a, b, c, d, e) => C0.S5(a, b, c, d, e);
            this.Test(z);
        }

        [Test]
        public void TestI2() {
            Func<int, int, int> z = (a, b) => (new C0()).I2(a, b);
            this.Test(z);
        }

        [Test]
        public void TestI3() {
            Func<int, int, int, int> z = (a, b, c) => (new C0()).I3(a, b, c);
            this.Test(z);
        }

        [Test]
        public void TestI4() {
            Func<int, int, int, int, int> z = (a, b, c, d) => (new C0()).I4(a, b, c, d);
            this.Test(z);
        }

        [Test]
        public void TestI5() {
            Func<int, int, int, int, int, int> z = (a, b, c, d, e) => (new C0()).I5(a, b, c, d, e);
            this.Test(z);
        }

        [Test]
        public void TestT2() {
            Func<int, int, int, int> z = (a, b, c) => (new C0 { i = a }).T2(b, c);
            this.Test(z);
        }

        [Test]
        public void TestT3() {
            Func<int, int, int, int, int> z = (a, b, c, d) => (new C0 { i = a }).T3(b, c, d);
            this.Test(z);
        }

        [Test]
        public void TestT4() {
            Func<int, int, int, int, int, int> z = (a, b, c, d, e) => (new C0 { i = a }).T4(b, c, d, e);
            this.Test(z);
        }

        [Test]
        public void TestT5() {
            Func<int, int, int, int, int, int, int> z = (a, b, c, d, e, f) => (new C0 { i = a }).T5(b, c, d, e, f);
            this.Test(z);
        }

    }

}
