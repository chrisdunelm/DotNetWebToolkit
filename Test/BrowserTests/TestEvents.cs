using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestEvents : BrowserTestBase {

        [Test]
        public void TestImageOnLoad() {
            Action f = () => {
                var img = new HtmlImageElement();
                img.OnLoad = () => {
                    Fail();
                };
                img.OnLoad = () => {
                    Window.SetTimeout(() => Pass(), 50);
                };
                img.Src = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
            };
            this.Start(f);
        }

    }

}
