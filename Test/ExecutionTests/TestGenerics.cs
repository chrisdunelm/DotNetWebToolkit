using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestGenerics : ExecutionTestBase {

        class C1<T> {
            public T value;
            public int i;
        }

        [Test]
        public void TestGeneric1() {
            Func<int, int> f = a => {
                var c = new C1<int> { value = a, i = a };
                var d = new C1<bool> { value = a > 50, i = 2 };
                return c.value + (d.value ? 10 : -10) + c.i + d.i;
            };
            this.Test(f);
        }

        class C2<T, U, V, W> {
            public T t;
            public U u;
            public V v;
            public W w;
        }

        [Test]
        public void TestGenericMultipleArgs() {
            Func<int, int> f = a => {
                var c = new C2<int, int, int, int> { t = a, u = a, v = a, w = a };
                return c.t + c.u + c.v + c.w;
            };
            this.Test(f);
        }

        abstract class C3<T> {
            public T t;
        }
        class C3A : C3<int> {
            public int i;
        }

        [Test]
        public void TestGenericInheritance() {
            Func<int, int, int> f = (a, b) => {
                var c = new C3A { t = a, i = b };
                return c.t + c.i;
            };
            this.Test(f);
        }

        abstract class C4<T, U> {
            public T t;
            public U u;
        }
        class C4A<T> : C4<T, int> {
            public T t2;
        }

        [Test]
        public void TestGenericInheritance2() {
            Func<int, int> f = a => {
                var c = new C4A<int> { t = a, t2 = a, u = a };
                return c.t + c.t2 + c.u;
            };
            this.Test(f);
        }

        class C5<T> {
            public T t;
            public T Get() {
                return this.t;
            }
        }

        [Test]
        public void TestWithGenericMethod() {
            Func<int, int> f = a => {
                var c = new C5<int> { t = a };
                return c.Get();
            };
            this.Test(f);
        }

        static class C6 {
            public static T Get<T>(T t) {
                return t;
            }
        }

        [Test]
        public void TestStatic() {
            Func<int, int> f = a => C6.Get(a);
            this.Test(f);
        }

    }

}
