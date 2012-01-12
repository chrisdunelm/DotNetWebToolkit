using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestString : ExecutionTestBase {

        [Test]
        public void TestEquality() {
            Func<string, string, bool> f = (a, b) => a == b;
            Func<int, int, bool> g = (a, b) => {
                var x = a > 50 ? "a" : "b";
                var y = b > 50 ? "a" : "b";
                return x == y;
            };
            this.Test(f, g);
        }

        [Test]
        public void TestLength() {
            Func<string, int> f = s => s.Length;
            this.Test(f);
        }

        [Test]
        public void TestCharAccess() {
            Func<int, char> f = a => "abcdefgh"[a & 7];
            this.Test(f);
        }

        [Test]
        public void TestConcat2() {
            Func<string, string, string> f = (a, b) => a + b;
            this.Test(f);
        }

        [Test]
        public void TestConcat3() {
            Func<string, string, string, string> f = (a, b, c) => a + b + c;
            this.Test(f);
        }

        [Test]
        public void TestConcat4() {
            Func<string, string, string, string, string> f = (a, b, c, d) => a + b + c + d;
            this.Test(f);
        }

        [Test]
        public void TestConcat5() {
            Func<string, string, string, string, string, string> f = (a, b, c, d, e) => a + b + c + d + e;
            this.Test(f);
        }

        [Test]
        public void TestConcat6() {
            Func<string, string, string, string, string, string, string> f = (a, b, c, d, e, z) => a + b + c + d + e + z;
            this.Test(f);
        }

        [Test]
        public void TestIndexOfChar() {
            Func<int, int> f = a => "aceg".IndexOf("abcdefgh"[a & 7]);
            this.Test(f);
        }

        [Test]
        public void TestIndexOfString() {
            Func<bool, bool, int> f = (b, c) => "abcdefgh".IndexOf(b ? (c ? "abc" : "") : (c ? "efg" : "z"));
            this.Test(f);
        }

        [Test]
        public void TestIndexOfCharStart() {
            Func<int, int> f = a => "abababab".IndexOf('a', a & 7);
            this.Test(f);
        }

        [Test]
        public void TestIndexOfStringStart() {
            Func<bool, bool, int, int> f = (b, c, i) => "abcdefgh".IndexOf(b ? (c ? "abc" : "") : (c ? "efg" : "z"), i & 7);
            this.Test(f);
        }

        [Test]
        public void TestSubstring() {
            Func<int, string> f = i => "abcdefg".Substring(i & 7);
            this.Test(f);
        }

        [Test]
        public void TestSubstringWithLength() {
            Func<int, int, string> f = (i, j) => "abcdefg".Substring(i & 3, j & 3);
            this.Test(f);
        }

    }

}
