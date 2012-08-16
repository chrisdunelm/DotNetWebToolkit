using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestXmlHttpRequest : BrowserTestBase {

        [Test]
        public void TestJsSendBool() {
            bool okTrue = false, okFalse = false;
            this.SetUrlJson("/xhrTrue", data => { okTrue = base.GetJson().Decode<bool>(data) == true; });
            this.SetUrlJson("/xhrFalse", data => { okFalse = base.GetJson().Decode<bool>(data) == false; });
            Action f = () => {
                int recvCount = 0;
                Action<object> onRecv = data => {
                    if (++recvCount == 2) {
                        Pass();
                    }
                };
                var xhrTrue = new XmlHttpRequest2<bool, object>("xhrTrue", onRecv);
                var xhrFalse = new XmlHttpRequest2<bool, object>("xhrFalse", onRecv);
                xhrTrue.Send(true);
                xhrFalse.Send(false);
            };
            this.Start(f, null, wd => okTrue && okFalse);
        }

        [Test]
        public void TestJsSendBoolNullable() {
            bool isNull = false;
            this.SetUrlJson("/xhr", data => { isNull = base.GetJson().Decode<bool?>(data) == null; });
            Action f = () => {
                var xhr = new XmlHttpRequest2<bool?, object>("xhr", data => Pass());
                xhr.Send(null);
            };
            this.Start(f, null, wd => isNull);
        }

        [Test]
        public void TestJsSendInt32() {
            int sum = 0;
            this.SetUrlJson("/xhr", data => { sum += base.GetJson().Decode<int>(data); });
            Action f = () => {
                int recvCount = 0;
                var xhr = new XmlHttpRequest2<int, object>("xhr", data => {
                    if (++recvCount == 5) {
                        Pass();
                    }
                });
                for (int i = 0; i < 5; i++) {
                    xhr.Send(i);
                }
            };
            this.Start(f, null, wd => sum == 10);
        }

        [Test]
        public void TestJsSendInt64() {
            Int64 sum = 0;
            this.SetUrlJson("/xhr", data => { sum += base.GetJson().Decode<Int64>(data); });
            Action f = () => {
                int recvCount = 0;
                var xhr = new XmlHttpRequest2<Int64, object>("xhr", data => {
                    if (++recvCount == 5) {
                        Pass();
                    }
                });
                for (var i = 0; i < 5; i++) {
                    xhr.Send(i);
                }
            };
            this.Start(f, null, wd => sum == 10);
        }

        [Test]
        public void TestJsSendChar() {
            string s = "";
            this.SetUrlJson("/xhr", data => { s += base.GetJson().Decode<char>(data); });
            Action f = () => {
                var toSend = new[] { 'A', 'B', 'C' };
                int recvCount = 0;
                var xhr = new XmlHttpRequest2<char, object>("xhr", data => {
                    if (++recvCount == toSend.Length) {
                        Pass();
                    }
                });
                for (int i = 0; i < toSend.Length; i++) {
                    xhr.Send(toSend[i]);
                }
            };
            this.Start(f, null, wd => s == "ABC");
        }

        [Test]
        public void TestJsSendString() {
            string s = "";
            this.SetUrlJson("/xhr", data => { s += base.GetJson().Decode<string>(data); });
            Action f = () => {
                var toSend = new[] { "A", "B", "C" };
                int recvCount = 0;
                var xhr = new XmlHttpRequest2<string, object>("xhr", data => {
                    if (++recvCount == toSend.Length) {
                        Pass();
                    }
                });
                for (int i = 0; i < toSend.Length; i++) {
                    xhr.Send(toSend[i]);
                }
            };
            this.Start(f, null, wd => s == "ABC");
        }

        [Test]
        public void TestJsSendStringNull() {
            string s = "";
            this.SetUrlJson("/xhr", data => { s = base.GetJson().Decode<string>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<string, object>("xhr", data => Pass());
                xhr.Send(null);
            };
            this.Start(f, null, wd => s == null);
        }

        [Test]
        public void TestJsSendNullInt32Array() {
            int[] res = new int[1];
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<int[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<int[], object>("xhr", data => Pass());
                xhr.Send(null);
            };
            this.Start(f, null, wd => res == null);
        }

        [Test]
        public void TestJsSendInt32Array() {
            int[] res = null;
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<int[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<int[], object>("xhr", data => Pass());
                xhr.Send(new[] { 0, 1, 99 });
            };
            this.Start(f, null, wd => res.Length == 3 && res[0] == 0 && res[1] == 1 && res[2] == 99);
        }

        [Test]
        public void TestJsSendInt64Array() {
            Int64[] res = null;
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<Int64[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<Int64[], object>("xhr", data => Pass());
                xhr.Send(new Int64[] { 0x100000008L, -2, 99 });
            };
            this.Start(f, null, wd => res.Length == 3 && res[0] == 0x100000008L && res[1] == -2 && res[2] == 99);
        }

        [Test]
        public void TestJsSendInt32NullableArray() {
            int?[] res = null;
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<int?[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<int?[], object>("xhr", data => Pass());
                xhr.Send(new int?[] { null, 1, null });
            };
            this.Start(f, null, wd => res.Length == 3 && res[0] == null && res[1] == 1 && res[2] == null);
        }

        [Test]
        public void TestJsSendInt64NullableArray() {
            Int64?[] res = null;
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<Int64?[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<Int64?[], object>("xhr", data => Pass());
                xhr.Send(new Int64?[] { null, 1, null });
            };
            this.Start(f, null, wd => res.Length == 3 && res[0] == null && res[1] == 1 && res[2] == null);
        }

        [Test]
        public void TestJsSendStringArray() {
            string[] res = null;
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<string[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<string[], object>("xhr", data => Pass());
                xhr.Send(new[] { "ABC", "123", "" });
            };
            this.Start(f, null, wd => res.Length == 3 && res[0] == "ABC" && res[1] == "123" && res[2] == "");
        }

        [Test]
        public void TestJsSendStringNullArray() {
            string[] res = null;
            this.SetUrlJson("/xhr", data => { res = base.GetJson().Decode<string[]>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<string[], object>("xhr", data => {
                    Pass();
                });
                xhr.Send(new[] { "ABC", null, null });
            };
            this.Start(f, null, wd => res.Length == 3 && res[0] == "ABC" && res[1] == null && res[2] == null);
        }

        class SendObjectWithPrimitives {
            public Int32 i;
            public Int64 l;
            public string s;
            public char c;
        }
        [Test]
        public void TestJsSendObjectWithPrimitives() {
            SendObjectWithPrimitives obj = null;
            this.SetUrlJson("/xhr", data => { obj = base.GetJson().Decode<SendObjectWithPrimitives>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<SendObjectWithPrimitives, object>("xhr", data => Pass());
                var o = new SendObjectWithPrimitives { i = -2, l = -4, s = "abc", c = 'Z' };
                xhr.Send(o);
            };
            this.Start(f, null, wd => obj.i == -2 && obj.l == -4 && obj.s == "abc" && obj.c == 'Z');
        }

        class SendObjectWithReference {
            public class Inner {
                public int i;
                public char c;
            }
            public Inner inner;
            public Inner innerNull;
            public string s;
        }
        [Test]
        public void TestJsSendObjectWithReferenceWithNull() {
            SendObjectWithReference obj = null;
            this.SetUrlJson("/xhr", data => { obj = base.GetJson().Decode<SendObjectWithReference>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<SendObjectWithReference, object>("xhr", data => Pass());
                var o = new SendObjectWithReference {
                    s = "abc",
                    inner = new SendObjectWithReference.Inner { i = 3, c = 'z' },
                    innerNull = null,
                };
                xhr.Send(o);
            };
            this.Start(f, null, wd => obj.s == "abc" && obj.inner.i == 3 && obj.inner.c == 'z' && obj.innerNull == null);
        }

        class SendObjectWithSelfReference {
            public SendObjectWithSelfReference self;
        }
        [Test]
        public void TestJsSendObjectWithSelfReference() {
            SendObjectWithSelfReference obj = null;
            this.SetUrlJson("/xhr", data => { obj = base.GetJson().Decode<SendObjectWithSelfReference>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<SendObjectWithSelfReference, object>("xhr", data => Pass());
                var o = new SendObjectWithSelfReference();
                o.self = o;
                xhr.Send(o);
            };
            this.Start(f, null, wd => obj.self == obj);
        }

        class SendObjectsWithMutalRefs1 {
            public SendObjectsWithMutalRefs1 self;
            public SendObjectsWithMutalRefs2 other;
        }
        class SendObjectsWithMutalRefs2 {
            public SendObjectsWithMutalRefs2 self;
            public SendObjectsWithMutalRefs1 other;
        }
        [Test]
        public void TestJsSendObjectsWithMutalRefs() {
            SendObjectsWithMutalRefs1 obj = null;
            this.SetUrlJson("/xhr", data => { obj = base.GetJson().Decode<SendObjectsWithMutalRefs1>(data); });
            Action f = () => {
                var xhr = new XmlHttpRequest2<SendObjectsWithMutalRefs1, object>("xhr", data => Pass());
                var o1 = new SendObjectsWithMutalRefs1();
                var o2 = new SendObjectsWithMutalRefs2();
                o1.self = o1;
                o1.other = o2;
                o2.self = o2;
                o2.other = o1;
                xhr.Send(o1);
            };
            this.Start(f, null, wd => obj.self == obj && obj.other.other == obj && obj.other.self == obj.other);
        }

    }

}
