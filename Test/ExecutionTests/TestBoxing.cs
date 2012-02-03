using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestBoxing : ExecutionTestBase {

        [Test]
        public void TestSimpleBox() {
            Func<bool> f = () => {
                int i = 3;
                object o = i;
                return o.GetType() == typeof(int);
            };
            this.Test(f);
        }

        [Test]
        public void TestSimpleBoxUnbox() {
            Func<int, int> f = a => {
                object o = a;
                return (int)o;
            };
            this.Test(f);
        }

#pragma warning disable 0649
        struct S1 { public int x; }
#pragma warning restore 0649
        static object ForceObject<T>(T o) { return o; }
        static bool GetFalse(bool b) { return false; }

        [Test]
        public void TestUnboxNullStruct() {
            Func<bool> f = () => {
                var o = ForceObject((object)null);
                bool ret;
                try {
                    // Throws NullReferenceExeption("Object reference not set to an instance of an object.")
                    var s = (S1)o;
                    ret = GetFalse(s.x != 0);
                } catch (Exception e) {
                    ret = e.Message == "Object reference not set to an instance of an object.";
                }
                return ret;
            };
            this.Test(f);
        }

        static int Get3() { return 3; }

        [Test]
        public void TestUnboxToWrongType() {
            Func<bool> f = () => {
                object o = Get3();
                bool ret;
                try {
                    // Throws InvalidCastException("Specified cast is not valid.")
                    short s = (short)o;
                    ret = GetFalse(s != 0);
                } catch (Exception e) {
                    ret = e.Message == "Specified cast is not valid.";
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestUnboxNullableToWrongType() {
            Func<bool> f = () => {
                int? i = Get3();
                object o = i;
                bool ret;
                try {
                    // Throws InvalidCastException("Specified cast is not valid.")
                    short s = (short)o;
                    ret = GetFalse(s != 0);
                } catch (Exception e) {
                    ret = e.Message == "Specified cast is not valid.";
                }
                return ret;
            };
            this.Test(f);
        }

        [Test]
        public void TestUnboxNullableIntoNormal() {
            Func<bool> f = () => {
                int? i = Get3();
                object o = (object)i;
                int j = (int)o;
                return j == 3;
            };
            this.Test(f);
        }

        [Test]
        public void TestUnboxNormalIntoNullable() {
            Func<bool> f = () => {
                int i = Get3();
                object o = (object)i;
                int? j = (int)o;
                return j == 3;
            };
            this.Test(f);
        }

    }

}
