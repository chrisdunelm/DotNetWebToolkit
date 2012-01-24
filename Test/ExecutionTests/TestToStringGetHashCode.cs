using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

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

        [Test]
        public void TestInt32ToString() {
            Func<int, string> f = a => a.ToString();
            this.Test((Func<int, string>)TestInt32ToStringFunc);
        }
        private static string TestInt32ToStringFunc([ParamFullRange]int a) {
            return a.ToString();
        }

        [Test]
        public void TestDoubleGetHashCode() {
            this.Test((Func<double, double, bool>)TestDoubleGetHashCode);
        }
        private static bool TestDoubleGetHashCode([ParamFullRange]double a, [ParamFullRange]double b) {
            var ah = a.GetHashCode();
            var bh = b.GetHashCode();
            return a == b ? ah == bh : ah != bh;
        }

        [Test, Ignore("Double.ToString() not yet working")]
        public void TestDoubleToString() {
            this.Test((Func<double, string>)TestDoubleToString);
        }
        private static string TestDoubleToString([ParamFullRange]double a) {
            return a.ToString();
        }

    }

}
