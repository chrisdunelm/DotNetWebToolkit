using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestJsTypes : BrowserTestBase {

        [Test]
        public void TestJsItemProperty() {
            Action f = () => {
                var floatArray = new Float32Array(2);
                floatArray[0] = 7;
                floatArray[1] = 14;
                var sum = floatArray[0] + floatArray[1];
                Done(sum == 21);
            };
            this.Start(f);
        }

    }

}
