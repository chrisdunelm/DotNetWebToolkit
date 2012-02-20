using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests.Window_ {

    [TestFixture]
    public class TestRequestAnimationFrame : BrowserTestBase {

        [Test]
        public void Test1() {
            Action f = () => {
                Window.RequestAnimationFrame(() => {
                    Pass();
                });
            };
            this.Start(f);
        }

    }

}
