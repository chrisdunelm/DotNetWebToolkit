using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestSwitch : ExecutionTestBase {

        [Test]
        public void TestSwitch1() {
            Func<int, int> f = a => {
                switch (a & 3) {
                case 0: return 6;
                case 1: return 8;
                default: return 26;
                }
            };
            this.Test(f);
        }

        [Test]
        public void TestSwitch2() {
            Func<int, int> f = a => {
                switch (a & 3) {
                case 0: return 6;
                case 1:
                case 2: return 8;
                default: return 26;
                }
            };
            this.Test(f);
        }

        [Test]
        public void TestSwitchSame() {
            Func<int, int> f = a => {
                switch (a & 3) {
                case 0: return 6;
                case 1:
                case 2: return 8;
                }
                return 26;
            };
            this.Test(f);
        }

        [Test]
        public void TestSwitchSame2() {
            Func<int, int> f = a => {
                switch (a & 7) {
                case 0: return 6;
                case 1:
                case 4: return 8;
                case 2:
                case 3:
                case 5: return 15;
                case 6:
                case 7: return -1;
                }
                return 26;
            };
            this.Test(f);
        }

        [Test]
        public void TestWithLoops() {
            Func<int, int> f = a => {
                var r = 0;
                switch (a & 3) {
                case 0:
                    for (int i = 0; i < a; i++) {
                        r++;
                    }
                    break;
                case 1:
                    for (int j = a; j >= 0; j--) {
                        r--;
                    }
                    break;
                case 2:
                case 3:
                    for (; ; ) {
                        r++;
                        if (r >= 10) {
                            break;
                        }
                    }
                    break;
                default:
                    r = 1;
                    break;
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void TestNested() {
            Func<int, int, int> f = (a, b) => {
                switch (a & 3) {
                case 0:
                    switch (b & 3) {
                    case 0: return -1;
                    case 1: return 1;
                    case 2: return 6;
                    default: return 19;
                    }
                case 1:
                    switch (b & 7) {
                    case 0:
                    case 3:
                    case 5: return b;
                    case 1: return a;
                    case 2: return 0;
                    default: return 99;
                    }
                case 2:
                    switch ((b + 2) & 3) {
                    case 0: return b;
                    case 1: return a;
                    case 2: return b + a;
                    default: return b - a;
                    }
                default:
                    switch (b & 3) {
                    case 0:
                        switch ((a + b) & 3) {
                        case 0: return 5;
                        case 1: return 66;
                        case 2: return 88;
                        default: return 11;
                        }
                    case 1:
                        switch ((a + b) & 3) {
                        case 0: return 15;
                        case 1: return 166;
                        case 2: return 188;
                        default: return 111;
                        }
                    case 2:
                        switch ((a + b) & 3) {
                        case 0: return 25;
                        case 1: return 266;
                        case 2: return 288;
                        default: return 211;
                        }
                    default:
                        switch ((a + b) & 3) {
                        case 0: return 35;
                        case 1: return 366;
                        case 2: return 388;
                        default: return 311;
                        }
                    }
                }
            };
            this.Test(f);
        }

        [Test]
        public void TestWithThrow() {
            Func<int, int> f = a => {
                try {
                    switch (a & 3) {
                    case 0: return 0;
                    case 1: return 7;
                    case 2: return 18;
                    default: throw new Exception();
                    }
                } catch {
                    return -1;
                }
            };
            this.Test(f);
        }

        [Test, Ignore("Phi generation fails due to blocks being processing in wrong order")]
        public void TestWithFlag() {
            Func<int, int> f = a => {
                bool flag = false;
                var r = 0;
                switch (a & 3) {
                case 0:
                    if (a > 50) {
                        flag = true;
                    }
                    r = 6;
                    break;
                case 1:
                    if (a > 25) {
                        flag = true;
                    }
                    r = -1;
                    break;
                }
                if (flag) {
                    r++;
                }
                return r;
            };
            this.Test(f);
        }

    }

}
