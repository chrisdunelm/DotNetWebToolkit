using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestDelegates : ExecutionTestBase {


        delegate int Del1(int a);
        private static int Mul2Del1(int a) {
            return a * 2;
        }
        private static Del1 GetMul2Del1() {
            return Mul2Del1;
        }
        [Test]
        public void TestCustomDelegate() {
            Func<int, int> f = a => {
                var d = GetMul2Del1();
                return d(a);
            };
            this.Test(f);
        }

    }

}
