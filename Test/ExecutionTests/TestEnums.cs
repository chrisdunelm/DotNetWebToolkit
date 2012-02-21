using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestEnums : ExecutionTestBase {

        enum A { a = 1, b = 2 };
        enum B { c = 3, d = 4 };
        A ForceA(int i) { return (A)i; }
        B ForceB(int i) { return (B)i; }
        B ForceB(A a) { return (B)a; }
        public void TestCast() {
            Func<int, int> f = a => {
                var aa = ForceA(a);
                var bb = ForceB(a);
                var c = ForceB(aa);
                return (int)aa + (int)bb + (int)c;
            };
            this.Test(f);
        }

    }

}
