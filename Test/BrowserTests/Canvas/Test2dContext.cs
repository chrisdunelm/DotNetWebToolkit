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
                var ctx2 = canvas.GetContext2D();
                ctx2.FillStyle = "#ffeedd";
                Done((string)ctx2.FillStyle == "#ffeedd");
            };
            this.Start(f);
        }

        private static TextAlign GetEnd() { return TextAlign.End; }

        [Test]
        public void TestTextAlign() {
            this.HtmlBody = "<canvas id='x'></canvas>";
            Action f = () => {
                var canvas = (HtmlCanvasElement)Document.GetElementById("x");
                var ctx2 = canvas.GetContext2D();
                ctx2.TextAlign = TextAlign.Center;
                if (ctx2.TextAlign != TextAlign.Center) {
                    Fail();
                }
                ctx2.TextAlign = GetEnd();
                if (ctx2.TextAlign != TextAlign.End) {
                    Fail();
                }
                Pass();
            };
            this.Start(f);
        }

    }

}
