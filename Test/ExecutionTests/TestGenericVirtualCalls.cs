using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestGenericVirtualCalls : ExecutionTestBase {

        class C1<T> {
            public virtual T M(T t1, T t2) { return t1; }
            public class A : C1<T> {
                public override T M(T t1, T t2) { return t2; }
            }
        }

        [Test]
        public void TestGenericTypeVCall() {
            Func<bool, int, int, int> f = (b, x, y) => {
                var c = b ? new C1<int>() : new C1<int>.A();
                return c.M(x, y);
            };
            this.Test(f);
        }

        class C2 {
            public virtual T M<T>(T t1, T t2) { return t1; }
            public class A : C2 {
                public override T M<T>(T t1, T t2) { return t2; }
            }
        }

        [Test]
        public void TestGenericMethodVCall() {
            Func<bool, int, int, int> f = (b, x, y) => {
                var c = b ? new C2() : new C2.A();
                return c.M<int>(x, y);
            };
            this.Test(f);
        }

        [Test]
        public void TestGenericMethodVCallMultipleInstantiations() {
            Func<bool, int, int, int> f = (b, x, y) => {
                var c = b ? new C2() : new C2.A();
                var i = c.M<int>(x, y);
                var j = c.M<bool>(x > 50, y > 50) ? 100 : -100;
                return i + j;
            };
            this.Test(f);
        }

        class C3<T> {
            public virtual T M<U>(U x, U y) where U : T { return x; }
            public class A<TA> : C3<TA> {
                public override TA M<U>(U x, U y) { return y; }
            }
        }

        [Test]
        public void TestGenericMethodInGenericTypeVCall() {
            Func<bool, int, int, int> f = (b, x, y) => {
                var c = b ? new C3<int>() : new C3<bool>.A<int>();
                return c.M<int>(x, y);
            };
            this.Test(f);
        }

    }

}
