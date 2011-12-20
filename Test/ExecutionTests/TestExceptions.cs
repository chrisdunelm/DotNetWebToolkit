using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestExceptions : ExecutionTestBase {

        [Test,Ignore]
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

        [Test, Ignore]
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

        [Test, Ignore]
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

        [Test, Ignore]
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

        [Test, Ignore("Goto's not really supported")]
        public void TestWithGoto() {
            Func<bool, int> f = a => {
                try {
                    goto t;
                } catch {
                }
                t:
                try {
                    return 6;
                } catch {
                }
                return 5;
            };
            this.Test(f);
        }

    }

}
