using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestNullables : ExecutionTestBase {

        [Test]
        public void TestJustNull() {
            Func<bool> f = () => {
                int? i = null;
                return !i.HasValue;
            };
            this.Test(f);
        }

        [Test]
        public void TestJustNotNull() {
            Func<bool> f = () => {
                int? i = 3;
                return i.HasValue;
            };
            this.Test(f);
        }

        static int? GetNull() {
            return null;
        }

        [Test]
        public void TestObjectNull() {
            Func<bool> f = () => {
                int? i = GetNull();
                object o = (object)i;
                return o == null;
            };
            this.Test(f);
        }

        static int? Get3() {
            return 3;
        }

        [Test]
        public void TestObjectNotNull() {
            Func<bool> f = () => {
                int? i = Get3();
                object o = (object)i;
                return o != null;
            };
            this.Test(f);
        }

        [Test]
        public void TestBoxUnboxNull() {
            Func<bool> f = () => {
                int? i = GetNull();
                object o = (object)i;
                if (o != null) {
                    return false;
                }
                int? j = (int?)o;
                return j.HasValue == false;
            };
            this.Test(f);
        }

        [Test]
        public void TestBoxUnboxNotNull() {
            Func<bool> f = () => {
                int? i = Get3();
                object o = (object)i;
                if (o == null) {
                    return false;
                }
                int? j = (int?)o;
                return j == 3;
            };
            this.Test(f);
        }

    }

}
