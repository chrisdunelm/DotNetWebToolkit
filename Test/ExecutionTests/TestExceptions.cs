using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestExceptions : ExecutionTestBase {

        [Test]
        public void TestThrowReturnDirect() {
            Func<bool, int> f = b => {
                try {
                    if (b) {
                        throw new Exception();
                    }
                } catch {
                    return 1;
                }
                return 0;
            };
            this.Test(f);
        }

        [Test]
        public void TestThrowReturnVar() {
            Func<bool, int> f = b => {
                int r;
                try {
                    if (b) {
                        throw new Exception();
                    }
                    r = 1;
                } catch {
                    r = 2;
                }
                return r;
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
