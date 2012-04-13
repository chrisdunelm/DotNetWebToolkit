using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestStringBuilder : ExecutionTestBase {

        [Test]
        public void TestToStringOnly() {
            Func<string> f = () => (new StringBuilder()).ToString();
            this.Test(f);
        }

        [Test]
        public void TestAppendString() {
            Func<string, string> f = a => (new StringBuilder()).Append(a).Append(a).ToString();
            this.Test(f);
        }

        [Test]
        public void TestGetLength() {
            Func<int> f = () => {
                var sb = new StringBuilder();
                sb.Append("12");
                return sb.Length;
            };
            this.Test(f, 2);
        }

        [Test]
        public void TestSetLengthShorter() {
            Func<string> f = () => {
                var sb = new StringBuilder();
                sb.Append("abcd");
                sb.Length--;
                return sb.ToString();
            };
            this.Test(f, "abc");
        }

        [Test]
        public void TestSetLengthLonger() {
            Func<string> f = () => {
                var sb = new StringBuilder();
                sb.Append("abcd");
                sb.Length++;
                return sb.ToString();
            };
            this.Test(f, "abcd\u0000");
        }

    }

}
