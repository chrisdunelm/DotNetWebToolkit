using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web.WebGLHelpers;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestJsTypes : BrowserTestBase {

        [Test]
        public void TestJsItemProperty() {
            Action f = () => {
                var mat4 = new Mat4();
                mat4.Identity();
                mat4[1] = 7;
                mat4[2] = 14;
                var sum = mat4[0] + mat4[1] + mat4[2];
                Done(sum == 22);
            };
            this.Start(f);
        }

    }

}
