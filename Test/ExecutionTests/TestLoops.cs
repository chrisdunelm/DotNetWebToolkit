using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestLoops : ExecutionTestBase {

        [Test]
        public void Test1For() {
            Func<int, int, int> f = (a, b) => {
                for (int i = 0; i < a; i++) {
                    b++;
                }
                return b;
            };
            this.Test(f);
        }

        [Test]
        public void Test1Do() {
            Func<int, int, int> f = (a, b) => {
                do {
                    b++;
                    a--;
                } while (a > 0);
                return b;
            };
            this.Test(f);
        }

        [Test]
        public void Test1While() {
            Func<int, int, int> f = (a, b) => {
                while (a > 0) {
                    b++;
                    a--;
                }
                return b;
            };
            this.Test(f);
        }

        [Test]
        public void Test3NestedFors() {
            Func<int, int> f = a => {
                int r = 0;
                for (int i = 0; i < a; i++) {
                    for (int j = 0; j < 2; j++) {
                        for (int k = 0; k < 2; k++) {
                            r++;
                        }
                    }
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void Test3NestedForsWithExtraStatements() {
            Func<int, int> f = a => {
                int r = 0;
                for (int i = 0; i < a; i++) {
                    r--;
                    for (int j = 0; j < 2; j++) {
                        r--;
                        for (int k = 0; k < 2; k++) {
                            r++;
                        }
                        r++;
                    }
                    r++;
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void Test3NestedForsWithExtraIfs() {
            Func<int, int> f = a => {
                int r = 0;
                for (int i = 0; i < a; i++) {
                    if (r == 0) {
                        r--;
                    } else {
                        r++;
                    }
                    for (int j = 0; j < 2; j++) {
                        if (j == 0) {
                            r--;
                        }
                        for (int k = 0; k < 2; k++) {
                            r++;
                        }
                        if (j != 0) {
                            r++;
                        }
                    }
                    if (r < 3) {
                        r++;
                    }
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void Test1ForBreak() {
            Func<int, int> f = a => {
                for (int i = 0; i < 10; i++) {
                    if (i > 5) {
                        break;
                    }
                    a++;
                }
                return a;
            };
            this.Test(f);
        }

        [Test]
        public void Test1ForContinue() {
            Func<int, int> f = a => {
                for (int i = 0; i < 10; i++) {
                    if (i > 5) {
                        continue;
                    }
                    a++;
                }
                return a;
            };
            this.Test(f);
        }

        [Test]
        public void Test1ForBreakAndContinue() {
            Func<int, int> f = a => {
                for (int i = 0; i < 10; i++) {
                    if (i > 5) {
                        break;
                    }
                    if (i < 2) {
                        continue;
                    }
                    a++;
                }
                return a;
            };
            this.Test(f);
        }

        [Test]
        public void Test1ForBreakAndContinueMultiple() {
            Func<int, int> f = a => {
                for (int i = 0; i < 10; i++) {
                    if (i == 9) {
                        break;
                    }
                    if (i == 1) {
                        continue;
                    }
                    a += 10;
                    if (i == 2) {
                        continue;
                    }
                    if (i == 8) {
                        break;
                    }
                    a += 100;
                    if (i == 5) {
                        continue;
                    }
                    a += 1000;
                }
                return a;
            };
            this.Test(f);
        }

        [Test]
        public void Test2ForBreakAndContinue() {
            Func<int, int> f = a => {
                for (int j = 0; j < 5; j++) {
                    if (j == 4) {
                        break;
                    }
                    if (j == 2) {
                        continue;
                    }
                    for (int i = 0; i < 10; i++) {
                        if (i > 5) {
                            break;
                        }
                        if (i < 2) {
                            continue;
                        }
                        a++;
                    }
                    if (j == 3) {
                        continue;
                    }
                    a += 100;
                }
                return a;
            };
            this.Test(f);
        }

        [Test]
        public void Test4ForBreakAndContinue() {
            Func<int, int> f = a => {
                for (int j = 0; j < 5; j++) {
                    if (j == 4) {
                        break;
                    }
                    if (j == 2) {
                        continue;
                    }
                    for (int i = 0; i < 10; i++) {
                        for (int k = 0; ; k++) {
                            for (int l = 0; ; l++) {
                                if (l <= 3) {
                                    continue;
                                }
                                if (l >= 5) {
                                    break;
                                }
                                a += 1000;
                            }
                            if (k == 0) {
                                continue;
                            }
                            if (k >= 0) {
                                break;
                            }
                        }
                        if (i > 5) {
                            break;
                        }
                        if (i < 2) {
                            continue;
                        }
                        a++;
                    }
                    if (j == 3) {
                        continue;
                    }
                    a += 100;
                }
                return a;
            };
            this.Test(f);
        }

    }

}
