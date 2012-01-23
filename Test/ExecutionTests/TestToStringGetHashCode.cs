using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestToStringGetHashCode : ExecutionTestBase {

        [Test]
        public void TestStringGetHashCode() {
            Func<string, string, bool> f = (a, b) => {
                var ah1 = a.GetHashCode();
                var ah2 = a.GetHashCode();
                var bh = b.GetHashCode();
                return ah1 == ah2 && ah1 != bh;
            };
            this.Test(f);
        }

        [Test]
        public void TestStringToString() {
            Func<string, string> f = a => a.ToString();
            this.Test(f);
        }

        [Test]
        public void TestBooleanGetHashCode() {
            Func<int> f = () => true.GetHashCode() + false.GetHashCode();
            this.Test(f);
        }

        [Test]
        public void TestBooleanToString() {
            Func<string> f = () => true.ToString() + false.ToString();
            this.Test(f);
        }

        [Test]
        public void TestCharGetHashCode() {
            Func<char, int> f = a => a.GetHashCode();
            this.Test(f);
        }

        [Test]
        public void TestCharToString() {
            Func<char, string> f = a => a.ToString();
            this.Test(f);
        }

        [Test]
        public void TestInt32GetHashCode() {
            Func<int, int> f = a => a.GetHashCode();
            this.Test(f);
        }

        [Test, Ignore("int32.ToString() not yet supported")]
        public void TestInt32ToString() {
            Func<int, string> f = a => a.ToString();
            this.Test(f);
        }

    }

}
