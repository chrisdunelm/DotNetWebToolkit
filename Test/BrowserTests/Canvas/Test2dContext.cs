using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests.Canvas {

    [TestFixture]
    public class Test2dContext : BrowserTestBase {

        [Test]
        public void TestFillStyle() {
            this.HtmlBody = "<canvas id='x'></canvas>";
            Action f = () => {
                var canvas = (HtmlCanvasElement)Document.GetElementById("x");
                var ctx2 = (CanvasRenderingContext2D)canvas.GetContext(CanvasContext.TwoD);
                ctx2.FillStyle = "#ffeedd";
                Done(ctx2.FillStyle == "#ffeedd");
            };
            this.Start(f);
        }

    }

}
