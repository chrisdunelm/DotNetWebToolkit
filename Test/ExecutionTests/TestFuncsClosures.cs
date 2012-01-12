using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestFuncsClosures : ExecutionTestBase {

        class C0 {
            public static int i;
        }

        [Test]
        public void TestAction0NoClosure() {
            Func<int> f = () => {
                Action c = () => C0.i = 2;
                c();
                return C0.i;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction1NoClosure() {
            Func<int, int> f = a => {
                Action<int> c = ca => C0.i = ca;
                c(a);
                return C0.i;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction2NoClosure() {
            Func<int, int, int> f = (a, b) => {
                Action<int, int> c = (ca, cb) => C0.i = ca + cb;
                c(a, b);
                return C0.i;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction3NoClosure() {
            Func<int, int, int, int> f = (a, b, c) => {
                Action<int, int, int> z = (ca, cb, cc) => C0.i = ca + cb + cc;
                z(a, b, c);
                return C0.i;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction4NoClosure() {
            Func<int, int, int, int, int> f = (a, b, c, d) => {
                Action<int, int, int, int> z = (ca, cb, cc, cd) => C0.i = ca + cb + cc + cd;
                z(a, b, c, d);
                return C0.i;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction5NoClosure() {
            Func<int, int, int, int, int, int> f = (a, b, c, d, e) => {
                Action<int, int, int, int, int> z = (ca, cb, cc, cd, ce) => C0.i = ca + cb + cc + cd + ce;
                z(a, b, c, d, e);
                return C0.i;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction6NoClosure() {
            Func<int, int, int, int, int, int, int> y = (a, b, c, d, e, f) => {
                Action<int, int, int, int, int, int> z = (ca, cb, cc, cd, ce, cf) => C0.i = ca + cb + cc + cd + ce + cf;
                z(a, b, c, d, e, f);
                return C0.i;
            };
            this.Test(y);
        }

        [Test]
        public void TestAction7NoClosure() {
            Func<int, int, int, int, int, int, int, int> y = (a, b, c, d, e, f, g) => {
                Action<int, int, int, int, int, int, int> z = (ca, cb, cc, cd, ce, cf, cg) => C0.i = ca + cb + cc + cd + ce + cf + cg;
                z(a, b, c, d, e, f, g);
                return C0.i;
            };
            this.Test(y);
        }

        [Test]
        public void TestAction8NoClosure() {
            Func<int, int, int, int, int, int, int, int, int> y = (a, b, c, d, e, f, g, h) => {
                Action<int, int, int, int, int, int, int, int> z = (ca, cb, cc, cd, ce, cf, cg, ch) => C0.i = ca + cb + cc + cd + ce + cf + cg + ch;
                z(a, b, c, d, e, f, g, h);
                return C0.i;
            };
            this.Test(y);
        }

        [Test]
        public void TestAction0Closure() {
            Func<int> f = () => {
                int r = -1;
                Action c = () => r = 0;
                c();
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void TestAction1Closure() {
            Func<int, int> z = (a) => {
                int r = 0;
                Action<int> y = (n) => r = n;
                y(a);
                return r;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction2Closure() {
            Func<int, int, int> z = (a, b) => {
                int r = 0;
                Action<int, int> y = (n, o) => r = n + o;
                y(a, b);
                return r;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction3Closure() {
            Func<int, int, int, int> z = (a, b, c) => {
                int r = 0;
                Action<int, int, int> y = (n, o, p) => r = n + o + p;
                y(a, b, c);
                return r;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction4Closure() {
            Func<int, int, int, int, int> z = (a, b, c, d) => {
                int r = 0;
                Action<int, int, int, int> y = (n, o, p, q) => r = n + o + p + q;
                y(a, b, c, d);
                return r;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction5Closure() {
            Func<int, int, int, int, int, int> z = (a, b, c, d, e) => {
                int x = 0;
                Action<int, int, int, int, int> y = (n, o, p, q, r) => x = n + o + p + q + r;
                y(a, b, c, d, e);
                return x;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction6Closure() {
            Func<int, int, int, int, int, int, int> z = (a, b, c, d, e, f) => {
                int x = 0;
                Action<int, int, int, int, int, int> y = (n, o, p, q, r, s) => x = n + o + p + q + r + s;
                y(a, b, c, d, e, f);
                return x;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction7Closure() {
            Func<int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g) => {
                int x = 0;
                Action<int, int, int, int, int, int, int> y = (n, o, p, q, r, s, t) => x = n + o + p + q + r + s + t;
                y(a, b, c, d, e, f, g);
                return x;
            };
            this.Test(z);
        }

        [Test]
        public void TestAction8Closure() {
            Func<int, int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g, h) => {
                int x = 0;
                Action<int, int, int, int, int, int, int, int> y = (n, o, p, q, r, s, t, u) => x = n + o + p + q + r + s + t + u;
                y(a, b, c, d, e, f, g, h);
                return x;
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc0NoClosure() {
            Func<int> z = () => {
                Func<int> y = () => 3;
                return y();
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc1NoClosure() {
            Func<int, int> z = a => {
                Func<int, int> y = b => b;
                return y(a);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc2NoClosure() {
            Func<int, int, int> z = (a, b) => {
                Func<int, int, int> y = (n, o) => n + o;
                return y(a, b);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc3NoClosure() {
            Func<int, int, int, int> z = (a, b, c) => {
                Func<int, int, int, int> y = (n, o, p) => n + o + p;
                return y(a, b, c);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc4NoClosure() {
            Func<int, int, int, int, int> z = (a, b, c, d) => {
                Func<int, int, int, int, int> y = (n, o, p, q) => n + o + p + q;
                return y(a, b, c, d);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc5NoClosure() {
            Func<int, int, int, int, int, int> z = (a, b, c, d, e) => {
                Func<int, int, int, int, int, int> y = (n, o, p, q, r) => n + o + p + q + r;
                return y(a, b, c, d, e);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc6NoClosure() {
            Func<int, int, int, int, int, int, int> z = (a, b, c, d, e, f) => {
                Func<int, int, int, int, int, int, int> y = (n, o, p, q, r, s) => n + o + p + q + r + s;
                return y(a, b, c, d, e, f);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc7NoClosure() {
            Func<int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g) => {
                Func<int, int, int, int, int, int, int, int> y = (n, o, p, q, r, s, t) => n + o + p + q + r + s + t;
                return y(a, b, c, d, e, f, g);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc8NoClosure() {
            Func<int, int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g, h) => {
                Func<int, int, int, int, int, int, int, int, int> y = (n, o, p, q, r, s, t, u) => n + o + p + q + r + s + t + u;
                return y(a, b, c, d, e, f, g, h);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc0Closure() {
            Func<int, int> z = a => {
                Func<int> y = () => a;
                return y();
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc1Closure() {
            Func<int, int, int> z = (a, b) => {
                Func<int, int> y = (n) => a + n;
                return y(b);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc2Closure() {
            Func<int, int, int, int> z = (a, b, c) => {
                Func<int, int, int> y = (n, o) => a + n + o;
                return y(b, c);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc3Closure() {
            Func<int, int, int, int, int> z = (a, b, c, d) => {
                Func<int, int, int, int> y = (n, o, p) => a + n + o + p;
                return y(b, c, d);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc4Closure() {
            Func<int, int, int, int, int, int> z = (a, b, c, d, e) => {
                Func<int, int, int, int, int> y = (n, o, p, q) => a + n + o + p + q;
                return y(b, c, d, e);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc5Closure() {
            Func<int, int, int, int, int, int, int> z = (a, b, c, d, e, f) => {
                Func<int, int, int, int, int, int> y = (n, o, p, q, r) => a + n + o + p + q + r;
                return y(b, c, d, e, f);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc6Closure() {
            Func<int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g) => {
                Func<int, int, int, int, int, int, int> y = (n, o, p, q, r, s) => a + n + o + p + q + r + s;
                return y(b, c, d, e, f, g);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc7Closure() {
            Func<int, int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g, h) => {
                Func<int, int, int, int, int, int, int, int> y = (n, o, p, q, r, s, t) => a + n + o + p + q + r + s + t;
                return y(b, c, d, e, f, g, h);
            };
            this.Test(z);
        }

        [Test]
        public void TestFunc8Closure() {
            Func<int, int, int, int, int, int, int, int, int, int> z = (a, b, c, d, e, f, g, h, i) => {
                Func<int, int, int, int, int, int, int, int, int> y = (n, o, p, q, r, s, t, u) => a + n + o + p + q + r + s + t + u;
                return y(b, c, d, e, f, g, h, i);
            };
            this.Test(z);
        }

    }

}
