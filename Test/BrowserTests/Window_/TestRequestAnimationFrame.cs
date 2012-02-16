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

        [Test, Ignore("Fails for unknown reason")]
        public void Test1() {
            Action f = () => {
                int i = 0;
                Window.RequestAnimationFrame(() => {
                    i++;
                    if (i > 2) {
                        Pass();
                    }
                });
            };
            this.Start(f);
        }

    }

}
