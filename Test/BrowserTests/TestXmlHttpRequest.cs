using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestXmlHttpRequest : BrowserTestBase {

        [Test]
        public void TestSendRespString() {
            this.SetUrl("/xhr", () => "abc");
            Action f = () => {
                var xhr = new XMLHttpRequest();
                xhr.Open("GET", "xhr");
                xhr.OnReadyStateChange = () => {
                    if (xhr.ReadyState == XMLHttpRequestReadyState.Done) {
                        Done(xhr.ResponseText == "abc");
                    }
                };
                xhr.Send();
            };
            this.Start(f);
        }

    }

}
