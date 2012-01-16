using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    class TestSimpleInterfaces : ExecutionTestBase {

        class CTestInterface {
            public interface I {
                int GetA();
            }
            public interface I2 : I {
                int GetB();
            }
            public class A : I {
                public int GetA() { return 1; }
            }
            public class B : I {
                public int GetA() { return 2; }
            }
            public class C : B {
            }
            public class D : I2 {
                public int GetA() { return 3; }
                public int GetB() { return 4; }
            }
        }

        [Test]
        public void TestInterface() {
            Func<bool, int> f = b => {
                var i = b ? (CTestInterface.I)new CTestInterface.A() : (CTestInterface.I)new CTestInterface.B();
                return i.GetA();
            };
            this.Test(f);
        }

        [Test]
        public void TestInterfaceInheritedMethod() {
            Func<int> f = () => {
                CTestInterface.I o = new CTestInterface.C();
                return o.GetA();
            };
            this.Test(f);
        }

        [Test]
        public void TestInheritedInterface() {
            Func<int> f = () => {
                CTestInterface.I2 o = new CTestInterface.D();
                return o.GetA() + o.GetB();
            };
            this.Test(f);
        }

        class C2 {
            public interface I1 {
                int GetA();
            }
            public interface I2 : I1 {
                int GetB();
            }
            public interface I3 : I2 {
                int GetC();
            }
            public interface I4 : I1 {
                int GetD();
            }
            public interface I5 {
                int GetE();
            }
            public class A : I1 {
                public int GetA() { return 1; }
            }
            public abstract class B : A, I3 {
                public virtual int GetB() { return 11; }
                public abstract int GetC();
            }
            public class C : B {
                public override int GetC() { return 101; }
            }
            public class D : C, I4, I5 {
                public override int GetB() { return 1001; }
                public int GetD() { return 1002; }
                public int GetE() { return 1003; }
            }
        }

        [Test]
        public void TestInterfacesManyMethodCalls() {
            Func<int> f = () => {
                var a = new C2.A();
                var c = new C2.C();
                var d = new C2.D();
                return a.GetA() + c.GetA() + c.GetB() + c.GetC() + d.GetA() + d.GetB() + d.GetC() + d.GetD() + d.GetE();
            };
            this.Test(f);
        }

        [Test]
        public void TestInterfacesManyInterfaceCalls() {
            Func<int> f = () => {
                var a1 = (C2.I1)new C2.A();
                var c1 = (C2.I1)new C2.C();
                var c2 = (C2.I2)new C2.C();
                var c3 = (C2.I3)new C2.C();
                var d1 = (C2.I1)new C2.D();
                var d2 = (C2.I2)new C2.D();
                var d3 = (C2.I3)new C2.D();
                var d4 = (C2.I4)new C2.D();
                var d5 = (C2.I5)new C2.D();
                int a = a1.GetA();
                int c = c1.GetA() + c2.GetA() + c2.GetB() + c3.GetA() + c3.GetB() + c3.GetC();
                int d = d1.GetA() + d2.GetA() + d2.GetB() + d3.GetA() + d3.GetB() + d3.GetC() + d4.GetA() + d4.GetD() + d5.GetE();
                return a + c + d;
            };
            this.Test(f);
        }

        interface IExplicitInterface {
            int I { get; }
        }
        class CExplicitInterface1 : IExplicitInterface {
            public int I { get { return 0; } }
            int IExplicitInterface.I {
                get { return 3; }
            }
        }
        class CExplicitInterface2 : IExplicitInterface {
            int IExplicitInterface.I {
                get { return 4; }
            }
            public int I { get { return 0; } }
        }

        [Test]
        public void TestExplicitInterface() {
            Func<int> f = () => {
                IExplicitInterface i1 = new CExplicitInterface1();
                IExplicitInterface i2 = new CExplicitInterface2();
                return i1.I + i2.I;
            };
            this.Test(f);
        }

    }

}
