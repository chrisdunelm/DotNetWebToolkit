using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestCasting : ExecutionTestBase {

        private static object ForceObject(object o) {
            return o;
        }

        class CCast {
            public int v;
        }

        [Test]
        public void TestCast() {
            Func<int,int> f = a => {
                var c = new CCast { v = a };
                object o = ForceObject(c);
                var c2 = (CCast)o;
                return c2.v;
            };
            this.Test(f);
        }

        [Test]
        public void TestCastArray() {
            Func<bool> f = () => {
                var a = new[] { 1 };
                object o = ForceObject(a);
                var b = (int[])o;
                return b.GetType() == typeof(int[]);
            };
            this.TestTrue(f);
        }

        [Test]
        public void TestCastInterface() {
            Func<bool> f = () => {
                var a = new[] { 1 };
                object o = ForceObject(a);
                var i = (IList<int>)o;
                return i.Count == 1;
            };
            this.TestTrue(f);
        }

        [Test]
        public void TestIsType() {
            Func<bool> f = () => {
                object o = "string";
                if (!(o is string)) {
                    return false;
                }
                o = new object();
                if (o is string) {
                    return false;
                }
                return true;
            };
            this.TestTrue(f);
        }

        [Test]
        public void TestIsTypeValueType() {
            Func<bool> f = () => {
                object i = 3;
                return i is int;
            };
            this.TestTrue(f);
        }

    }

}
