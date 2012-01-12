using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestGenericInterfaces : ExecutionTestBase {

        class CGenericIFace {
            public interface I1<T> {
                T M(T t);
            }
            public class C1 : I1<int> {
                public int M(int t) { return t; }
            }
            public class C2<T> : I1<T> {
                public T M(T t) { return t; }
            }
        }

        [Test]
        public void TestGenericIFace1() {
            Func<int, int> f = a => {
                CGenericIFace.I1<int> i = new CGenericIFace.C1();
                return i.M(a);
            };
            this.Test(f);
        }

        [Test]
        public void TestGenericIFace2() {
            Func<int, int> f = a => {
                CGenericIFace.I1<int> i = new CGenericIFace.C2<int>();
                return i.M(a);
            };
            this.Test(f);
        }

        class CGenericMethodIFace {
            public interface I1 {
                T M<T>(T t);
            }
            public class C1 : I1 {
                public T M<T>(T t) { return t; }
            }
            public class C2<T> : I1 {
                public U M<U>(U t) { return t; }
            }
        }

        [Test]
        public void TestGenericMethodIFace1() {
            Func<int, int> f = a => {
                CGenericMethodIFace.I1 i = new CGenericMethodIFace.C1();
                return i.M<int>(a);
            };
            this.Test(f);
        }

        [Test]
        public void TestGenericMethodIFace2() {
            Func<int, int> f = a => {
                CGenericMethodIFace.I1 i = new CGenericMethodIFace.C2<bool>();
                return i.M<int>(a);
            };
            this.Test(f);
        }

    }

}
