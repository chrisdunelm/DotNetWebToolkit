using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestImage : BrowserTestBase {

        [Test]
        public void TestNewImage() {
            Action f = () => {
                var i = new HtmlImageElement();
                Done(i != null);
            };
            this.Start(f);
        }

    }

}
