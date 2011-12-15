using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    class TestSimpleInterfaces : ExecutionTestBase {

        class CTestInterface {
            public interface I {
                int GetA();
            }
            public class A : I {
                public int GetA() {
                    return 1;
                }
            }
            public class B : I {
                public int GetA() {
                    return 2;
                }
            }
        }

        [Test]
        public void TestInterface() {
            Func<bool, int> f = b => {
                var i = b ? (CTestInterface.I)new CTestInterface.A() : new CTestInterface.B();
                return i.GetA();
            };
            this.Test(f);
        }

    }

}
