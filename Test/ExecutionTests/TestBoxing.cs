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
            this.Test(f);
        }

        [Test]
        public void TestSimpleBoxUnbox() {
            Func<int, int> f = a => {
                object o = a;
                return (int)o;
            };
            this.Test(f);
        }

        [Test, Ignore("Statements can move in/out of try/catch/finally blocks. Which needs fixing")]
        public void TestUnboxToWrongType() {
            Func<bool> f = () => {
                object o = 3;
                try {
                    short s = (short)o;
                } catch {
                    return true;
                }
                return false;
            };
            this.Test(f);
        }

    }

}
