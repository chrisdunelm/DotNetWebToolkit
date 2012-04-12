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
        public void TestStringEmptySet() {
            // String has no static constructor to set String.Empty, so test the special-case code to set
            Func<string> f = () => string.Empty;
            this.Test(f);
        }

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
        public void TestConcatObject2() {
            Func<string, char, string> f = (a, b) => a + b;
            this.Test(f);
        }

        [Test]
        public void TestConcatObject3() {
            Func<string, char, char, string> f = (a, b, c) => a + b + c;
            this.Test(f);
        }

        [Test]
        public void TestConcatObject4() {
            Func<string, char, char, char, string> f = (a, b, c, d) => a + b + c + d;
            this.Test(f);
        }

        [Test]
        public void TestConcatObject5() {
            Func<string, char, char, char, char, string> f = (a, b, c, d, e) => a + b + c + d + e;
            this.Test(f);
        }

        [Test]
        public void TestConcatObject6() {
            Func<string, char, char, char, char, char, string> f = (a, b, c, d, e, z) => a + b + c + d + e + z;
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

        [Test]
        public void TestStartsWith() {
            Func<bool> f = () => {
                if ("abc".StartsWith("b")) {
                    return false;
                }
                return "abc".StartsWith("a");
            };
            this.Test(f, true);
        }

        [Test]
        public void TestEndsWith() {
            Func<bool> f = () => {
                if ("abc".EndsWith("b")) {
                    return false;
                }
                return "abc".EndsWith("c");
            };
            this.Test(f, true);
        }

        [Test]
        public void TestSplit() {
            Func<bool> f = () => {
                var split = @"a.b[c\def4\a".Split('.', '[', '\\', '4');
                if (split.Length != 6) {
                    return false;
                }
                return split[0] == "a" && split[1] == "b" && split[2] == "c" && split[3] == "def" && split[4] == "" && split[5] == "a";
            };
            this.Test(f, true);
        }

        [Test]
        public void TestSplitNullArg() {
            Func<bool> f = () => {
                var s = "a b\nc\u2004d".Split(null);
                return s.Length == 4 && s[0] == "a" && s[1] == "b" && s[2] == "c" && s[3] == "d";
            };
            this.Test(f, true);
        }

        [Test]
        public void TestSplitNoArgs() {
            Func<bool> f = () => {
                var s = "a b\nc\u1680d".Split();
                return s.Length == 4 && s[0] == "a" && s[1] == "b" && s[2] == "c" && s[3] == "d";
            };
            this.Test(f, true);
        }

        class CNullCheck<T> {
            public static bool IsNull(T v) { return v == null; }
        }
        [Test]
        public void TestNullCheck() {
            Func<bool> f = () => CNullCheck<string>.IsNull("");
            this.Test(f);
        }

        private static object ForceObject(object o) { return o; }
        private static bool GetFalse() { return false; }
        private static bool GetTrue() { return true; }

        [Test]
        public void TestEmptyStringAsObjectNullCheck() {
            // Test only works in Release build
            Func<bool> f = () => {
                var so = ForceObject("");
                bool b;
                if (so == null) {
                    b = GetTrue();
                } else {
                    b = GetFalse();
                }
                return !b;
            };
            this.Test(f);
        }

        private static T Force<T>(T o) { return o; }

        [Test]
        public void TestAsCharEnumerable() {
            Func<string, string> f = a => {
                string ret = "";
                var z = Force<IEnumerable<char>>(a);
                foreach (var c in z) {
                    ret = c.ToString() + ret;
                }
                return ret;
            };
            this.Test(f);
        }

    }

}
