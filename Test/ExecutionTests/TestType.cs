using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestType : ExecutionTestBase {

        [Test]
        public void TestRuntimeTypeToString() {
            Func<string> f = () => typeof(int).ToString();
            this.Test(f);
        }

        private static object GetAsType(Type t) { return t; }

        [Test]
        public void TestTypeToString() {
            Func<string> f = () => GetAsType(typeof(int)).ToString();
            this.Test(f);
        }

    }

}
