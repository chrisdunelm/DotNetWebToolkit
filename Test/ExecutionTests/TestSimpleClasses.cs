using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestSimpleClasses : ExecutionTestBase {

        class CTestNewObj {
            public CTestNewObj(int value) {
                this.value = value;
            }
            public int value;
        }

        [Test]
        public void TestNewObj() {
            Func<int, int> f = a => {
                var o = new CTestNewObj(a);
                return o.value;
            };
            this.Test(f);
        }

        class CTestProperty {
            public CTestProperty(int a, int b) {
                this.a = a;
                this.b = b;
            }
            private int a, b;
            public int Sum { get { return this.a + this.b; } }
        }

        [Test]
        public void TestProperty() {
            Func<int, int, int> f = (a, b) => {
                var o = new CTestProperty(a, b);
                return o.Sum;
            };
            this.Test(f);
        }

        class CTestDerived {
            public class D : CTestDerived {
                public D(int a, int b)
                    : base(a) {
                    this.b = b;
                }
                public int b;
            }
            protected CTestDerived(int a) {
                this.a = a;
            }
            public int a;
        }

        [Test]
        public void TestDerived() {
            Func<int, int, int> f = (a, b) => {
                var o = new CTestDerived.D(a, b);
                return o.a + o.b;
            };
            this.Test(f);
        }

        abstract class CTestVirtual {

            public static CTestVirtual Make(bool b) {
                return b ? (CTestVirtual)new CTestVirtual.A() : new CTestVirtual.B();
            }

            class A : CTestVirtual {
                public override int I {
                    get { return 1; }
                }
            }

            class B : CTestVirtual {
                public override int I {
                    get { return 2; }
                }
            }

            public abstract int I { get; }

        }

        [Test]
        public void TestVirtual() {
            Func<bool, int> f = b => {
                var o = CTestVirtual.Make(b);
                return o.I;
            };
            this.Test(f);
        }

        abstract class CTestVirtualComplex {

            public abstract class A : CTestVirtualComplex {

                public static new A Make(int i) {
                    if ((i & 1) == 0) {
                        return new B();
                    } else {
                        return new C();
                    }
                }

                public override int GetA() {
                    return 2;
                }

                public abstract int GetB();

            }

            class B : A {
                public override int GetA() {
                    return 3;
                }

                public override int GetB() {
                    return 101;
                }
            }

            class C : A {
                public override int GetB() {
                    return 102;
                }
            }

            class D : CTestVirtualComplex {
            }

            class E : D {
                public override int GetA() {
                    return 4;
                }
            }

            class F : E {
                public override int GetA() {
                    return 5;
                }
            }

            class G : F {
            }

            public static CTestVirtualComplex Make(int i) {
                var j = i & 3;
                if (j == 0) {
                    return new D();
                }
                if (j == 1) {
                    return new E();
                }
                if (j == 2) {
                    return new F();
                }
                return new G();
            }

            public virtual int GetA() {
                return 1;
            }

        }

        [Test]
        public void TestVirtualComplex() {
            Func<int, int> f = a => {
                var o1 = CTestVirtualComplex.Make(a);
                var o2 = CTestVirtualComplex.A.Make(a);
                return o1.GetA() + o2.GetB();
            };
            this.Test(f);
        }

        class CTestStaticCctorCalledOnlyOnce {
            static CTestStaticCctorCalledOnlyOnce() {
                i++;
            }
            public static int i;
            public static int GetI() {
                return i;
            }
        }

        [Test]
        public void TestStaticCctorCalledOnlyOnce() {
            Func<int> f = () => {
                int r = 0;
                for (int i = 0; i < 2; i++) {
                    r += CTestStaticCctorCalledOnlyOnce.GetI();
                }
                return r;
            };
            this.Test(f);
        }

        class CTestStaticFields {
            public static int i = 105;
        }

        [Test]
        public void TestStaticFields() {
            Func<int> f = () => CTestStaticFields.i;
            this.Test(f);
        }

        class C10 {
#pragma warning disable 0649
            public static int i;
#pragma warning restore 0649
        }

        [Test]
        public void TestStaticFieldNoInit() {
            Func<int> f = () => C10.i;
            this.Test(f);
        }

        class C11 {
#pragma warning disable 0649
            public int i;
#pragma warning restore 0649
        }

        [Test]
        public void TestFieldNoInit() {
            Func<int> f = () => {
                var c = new C11();
                return c.i;
            };
            this.Test(f);
        }

    }

}
