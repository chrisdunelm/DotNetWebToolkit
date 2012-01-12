using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

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

        [Test, Ignore("Nested try statements not yet working")]
        public void TestNestedCatch2() {
            Func<bool, bool, int> f = (a, b) => {
                try {
                    if (a) {
                        throw new Exception();
                    }
                    try {
                        if (b) {
                            throw new Exception();
                        }
                    } catch {
                        return 1;
                    }
                } catch {
                    return 2;
                }
                return 3;
            };
            this.Test(f);
        }

    }

}
