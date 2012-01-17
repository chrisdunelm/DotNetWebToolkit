using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestBoxing : ExecutionTestBase {

        [Test]
        public void TestSimpleBox() {
            Func<bool> f = () => {
                int i = 3;
                object o = i;
                return o.GetType() == typeof(int);
            };
            this.TestTrue(f);
        }

        [Test]
        public void TestSimpleBoxUnbox() {
            Func<int, int> f = a => {
                object o = a;
                return (int)o;
            };
            this.Test(f);
        }



    }

}
