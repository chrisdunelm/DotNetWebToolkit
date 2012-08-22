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

    }

}
