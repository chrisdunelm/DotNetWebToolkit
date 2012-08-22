using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestRecvXmlHttpRequest : BrowserTestBase {

        [Test]
        public void TestJsRecvBoolTrue() {
            this.SetUrlJson("/xhr", data => true);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, bool>("xhr", data => Done(data == true));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvBoolFalse() {
            this.SetUrlJson("/xhr", data => false);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, bool>("xhr", data => Done(data == false));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvBoolNullable() {
            this.SetUrlJson("/xhr", data => null);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, bool?>("xhr", data => Done(data == null));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvInt32() {
            this.SetUrlJson("/xhr", data => 78);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, Int32>("xhr", data => Done(data == 78));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvInt64() {
            this.SetUrlJson("/xhr", data => -2L);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, Int64>("xhr", data => Done(data == -2L));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvInt64Nullable() {
            this.SetUrlJson("/xhr", data => null);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, Int64?>("xhr", data => Done(data == null));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvChar() {
            this.SetUrlJson("/xhr", data => 'A');
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, char>("xhr", data => Done(data == 'A'));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvString() {
            this.SetUrlJson("/xhr", data => "abc");
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, string>("xhr", data => Done(data == "abc"));
                xhr.Send(null);
            };
            this.Start(f);
        }

        [Test]
        public void TestJsRecvStringNull() {
            this.SetUrlJson("/xhr", data => null);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, string>("xhr", data => Done(data == null));
                xhr.Send(null);
            };
            this.Start(f);
        }

        class RecvObjectWithNoRefs {
            public int i;
            public char c;
            public Int64 l;
            public string s;
        }
        [Test]
        public void TestJsRecvObjectWithNoRefs() {
            this.SetUrlJson("/xhr", data => new RecvObjectWithNoRefs { i = 9, c = 'A', l = -2L, s = "abc" });
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, RecvObjectWithNoRefs>("xhr", data => {
                    Done(data.i == 9 && data.c == 'A' && data.l == -2L && data.s == "abc");
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

        class RecvObjectWithRefs {
            public class Inner {
                public string s;
            }
            public Inner i1, i2;
            public Inner iNull;
        }
        [Test]
        public void TestJsRecvObjectWithRefs() {
            this.SetUrlJson("/xhr", data => new RecvObjectWithRefs {
                i1 = new RecvObjectWithRefs.Inner { s = "one" },
                i2 = new RecvObjectWithRefs.Inner { s = "two" },
                iNull = null,
            });
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, RecvObjectWithRefs>("xhr", data => {
                    Done(data.i1.s == "one" && data.i2.s == "two" && data.iNull == null);
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

        class RecvObjectSelfRef {
            public RecvObjectSelfRef self;
        }
        [Test]
        public void TestJsRecvObjectSelfRef() {
            var obj = new RecvObjectSelfRef();
            obj.self = obj;
            this.SetUrlJson("/xhr", data => obj);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, RecvObjectSelfRef>("xhr", data => {
                    Done(data.self == data);
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

        class RecvObjectsMutualRefs1 {
            public RecvObjectsMutualRefs1 self;
            public RecvObjectsMutualRefs2 other;
        }
        class RecvObjectsMutualRefs2 {
            public RecvObjectsMutualRefs2 self;
            public RecvObjectsMutualRefs1 other;
        }
        [Test]
        public void TestJsRecvObjectsMutualRefs() {
            var obj1 = new RecvObjectsMutualRefs1();
            var obj2 = new RecvObjectsMutualRefs2();
            obj1.self = obj1;
            obj1.other = obj2;
            obj2.self = obj2;
            obj2.other = obj1;
            this.SetUrlJson("/xhr", data => obj1);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, RecvObjectsMutualRefs1>("xhr", data => {
                    Done(data.self == data && data.other.self == data.other && data.other.other == data);
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

        struct RecvNestedStructs {
            public struct Nest3 {
                public Int64 l;
            }
            public struct Nest2 {
                public Nest3 n3;
            }
            public struct Nest1 {
                public Nest2 n2;
            }
            public Nest1 n1;
        }
        [Test]
        public void TestJsRecvNestedStructs() {
            var obj = new RecvNestedStructs {
                n1 = new RecvNestedStructs.Nest1 {
                    n2 = new RecvNestedStructs.Nest2 {
                        n3 = new RecvNestedStructs.Nest3 {
                            l = -3
                        }
                    }
                }
            };
            this.SetUrlJson("/xhr", data => obj);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, RecvNestedStructs>("xhr", data => {
                    Done(data.n1.n2.n3.l == -3);
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

        struct RecvAllInObject {
            public object n;
            public object x;
        }
        [Test]
        public void TestJsRecvAllInObject() {
            var obj = new RecvAllInObject {
                x = 8,
                n = new RecvAllInObject {
                    x = -4L,
                    n = new RecvAllInObject {
                        x = "abc",
                        n = new RecvAllInObject {
                            x = null,
                            n = null
                        }
                    }
                }
            };
            this.SetUrlJson("/xhr", data => obj);
            Action f = () => {
                var xhr = new XmlHttpRequest2<object, RecvAllInObject>("xhr", data => {
                    Done((int)data.x == 8 && (long)((RecvAllInObject)data.n).x == -4L &&
                        (string)((RecvAllInObject)((RecvAllInObject)data.n).n).x == "abc" &&
                        ((RecvAllInObject)((RecvAllInObject)((RecvAllInObject)data.n).n).n).x == null &&
                        ((RecvAllInObject)((RecvAllInObject)((RecvAllInObject)data.n).n).n).n == null);
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

    }

}
