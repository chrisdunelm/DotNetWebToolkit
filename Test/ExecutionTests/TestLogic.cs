using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestLogic : ExecutionTestBase {

        [Test]
        public void Test1IfBool() {
            Func<bool, bool> f = a => {
                if (a) {
                    return true;
                } else {
                    return false;
                }
            };
            this.Test(f);
        }

        [Test]
        public void Test1IfInt() {
            Func<int, bool> f = a => {
                if (a > 50) {
                    return true;
                } else {
                    return false;
                }
            };
            this.Test(f);
        }

        [Test]
        public void TestMultiAnd() {
            Func<int, int, int, int> f = (a, b, c) => {
                if (a >= -1 && a < 50 && b > 10 && b < 75 && c > 6 && c < 80) {
                    return a;
                }
                return b;
            };
            this.Test(f);
        }

        [Test]
        public void TestMultiOr() {
            Func<int, int> f = a => {
                if (a == -1 || a == 2 || a == 4 || a == 6 || a == 8 || a == 10 || a == 12 || a == 14 || a > 60) {
                    return a;
                }
                return a + 1;
            };
            this.Test(f);
        }

    }

}
