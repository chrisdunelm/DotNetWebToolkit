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

        class CNoInit<T> {
#pragma warning disable 0649
            public T t;
#pragma warning restore 0649
        }

        [Test]
        public void TestGenericFieldNoInit() {
            Func<int> f = () => {
                var c = new CNoInit<int>();
                return c.t;
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
        public void TestWithMethodWithGenericReturnType() {
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
            public static U Get2<U>(U u) {
                return Get<U>(u);
            }
            public static V Get3<V>(V v) {
                return Get2<V>(v);
            }
            public static W Get4<W>(W w) {
                return Get3<W>(w);
            }
        }

        [Test]
        public void TestWithStaticMethodWithGenericArgument() {
            Func<int, int> f = a => C6.Get(a);
            this.Test(f);
        }

        [Test]
        public void TestWithStaticMethodWithGenericArgument2() {
            Func<int, int> f = a => C6.Get2(a);
            this.Test(f);
        }

        [Test]
        public void TestWithStaticMethodWithGenericArgument4() {
            Func<int, int> f = a => C6.Get4(a);
            this.Test(f);
        }

        static class CGenericClassGenericMethodMulti<T> {
            public static U Get1<U>(U u) { return u; }
            public static V Get2<V>(V v) { return CGenericClassGenericMethodMulti<V>.Get1<V>(v); }
            public static W Get3<W>(W w) { return CGenericClassGenericMethodMulti<W>.Get2<W>(w); }
            public static X Get4<X>(X x) { return CGenericClassGenericMethodMulti<X>.Get3<X>(x); }
        }

        [Test]
        public void TestGenericClassGenericMethodMulti() {
            Func<int, int> f = a => CGenericClassGenericMethodMulti<int>.Get4<int>(a);
            this.Test(f);
        }

        class C7A<T, U> {
            public C7A(T t, U u) { this.t = t; this.u = u; }
            public T t;
            public U u;
        }
        static class C7<T> {
            public static C7A<T, U> Get<U>(T t, U u) {
                return new C7A<T, U>(t, u);
            }
        }

        [Test]
        public void TestStaticGenericMethodInGenericClass() {
            Func<int, bool, int> f = (a, b) => {
                var c = C7<int>.Get(a, b);
                return c.t + (c.u ? 1 : -1);
            };
            this.Test(f);
        }

        [Test]
        public void TestStaticGenericMethodInGenericClassWithObj() {
            Func<int, int, int> f = (a, b) => {
                var c = C7<int>.Get(a, new C7A<int, int>(a, b));
                return c.t + c.u.t + c.u.u;
            };
            this.Test(f);
        }

        class CStaticField<T> {
            public static T Value;
        }

        [Test]
        public void TestStaticGenericField() {
            Func<int, string, int> f = (i, s) => {
                CStaticField<int>.Value = i;
                CStaticField<string>.Value = s;
                return CStaticField<int>.Value + CStaticField<string>.Value.Length;
            };
            this.Test(f);
        }

    }

}
