using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestExceptions : ExecutionTestBase {

        [Test, Ignore("Exceptions not yet implemented")]
        public void TestThrow() {
            Action f = () => {
                throw new Exception();
            };
            this.Test(f);
        }

        [Test]
        public void TestTryCatchFinally() {
            Func<int> f = () => {
                int r;
                try {
                    r = 1;
                } catch {
                    r = 2;
                } finally {
                    r = 3;
                }
                return r;
            };
            this.Test(f);
        }

    }

}
