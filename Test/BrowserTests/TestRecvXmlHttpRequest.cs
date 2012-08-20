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
                var xhr = new XmlHttpRequest2<object, bool>("xhr", data => {
                    Done(data == true);
                });
                xhr.Send(null);
            };
            this.Start(f);
        }

    }

}
