using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestObjectBasics : ExecutionTestBase {

        [Test]
        public void TestGetHashCode() {
            Func<bool> f = () => {
                var a = new object();
                var ah1 = a.GetHashCode();
                var ah2 = a.GetHashCode();
                if (ah1 != ah2) {
                    return false;
                }
                var b = new object();
                var bh = b.GetHashCode();
                if (bh == ah1) {
                    return false;
                }
                return true;
            };
            this.Test();
        }

        [Test, Ignore("BaseType not supported")]
        public void TestBaseType() {
            Func<bool> f = () => {
                string s = "str";
                var sType = s.GetType();
                var baseType = sType.BaseType;
                return typeof(object) == baseType;
            };
            this.Test(f);
        }

    }

}
