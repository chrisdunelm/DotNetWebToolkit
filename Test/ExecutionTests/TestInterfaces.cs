using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestInterfaces : ExecutionTestBase {

        interface I1 { int Get(int i); }
        interface I2 { int Get(int i); }
        interface I3 { int Get(int i); }
        class C : I1, I2, I3 {
            public int Get(int i) { return i * 2; }
        }

        [Test]
        public void TestOneMethodImplementsMultipleInterfaces() {
            Func<int, int> f = a => {
                var c = new C();
                var i1 = (I1)c;
                var i2 = (I2)c;
                var i3 = (I3)c;
                return i1.Get(a) + i2.Get(a) + i3.Get(a);
            };
            this.Test(f);
        }

    }

}
