using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests.Canvas {

    [TestFixture]
    public class TestCanvasElement : BrowserTestBase {

        [Test]
        public void TestGetCanvas() {
            this.HtmlBody = "<canvas id='x'></canvas>";
            Action f = () => {
                var canvas = (CanvasElement)Window.Document.GetElementById("x");
                Done(canvas != null);
            };
            this.Start(f);
        }

    }

}
