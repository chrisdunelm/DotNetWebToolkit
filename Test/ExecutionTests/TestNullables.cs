using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestNullables : ExecutionTestBase {

        [Test]
        public void TestJustNull() {
            Func<bool> f = () => {
                int? i = null;
                return !i.HasValue;
            };
            this.Test(f, true);
        }

        [Test]
        public void TestJustNotNull() {
            Func<bool> f = () => {
                int? i = 3;
                return i.HasValue;
            };
            this.Test(f, true);
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
            this.Test(f, true);
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
            this.Test(f, true);
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
            this.Test(f, true);
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
            this.Test(f, true);
        }

        [Test]
        public void TestGetValue() {
            Func<int, bool, int> f = (i, b) => {
                var a = b ? (int?)i : null;
                return a.HasValue ? a.Value : -1;
            };
            this.Test(f);
        }

        [Test]
        public void TestGetValueOrDefault() {
            this.Test((Func<int, bool, int>)TestGetValueOrDefaultFunc);
        }
        private static int TestGetValueOrDefaultFunc([ParamFullRange]int i, bool b) {
            var a = b ? (int?)i : null;
            return a.GetValueOrDefault();
        }

        [Test]
        public void TestGetValueOrDefaultParam() {
            this.Test((Func<int, bool, int>)TestGetValueOrDefaultParamFunc);
        }
        private static int TestGetValueOrDefaultParamFunc([ParamFullRange]int i, bool b) {
            var a = b ? (int?)i : null;
            return a.GetValueOrDefault(-1);
        }

    }

}
