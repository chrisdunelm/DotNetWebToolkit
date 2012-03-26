using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests.Window_ {

    [TestFixture]
    public class TestPopups : BrowserTestBase {

        [Test]
        public void TestAlert() {
            Action f = () => {
                Window.Alert("Testing");
                Pass();
            };
            this.Start(f, wd => {
                var alert = wd.SwitchTo().Alert();
                var t = alert.Text;
                alert.Accept();
                return t == "Testing";
            });
        }

        [Test]
        public void TestConfirmOK() {
            Action f = () => {
                var result = Window.Confirm("Testing?");
                Done(result);
            };
            this.Start(f, wd => {
                var alert = wd.SwitchTo().Alert();
                var t = alert.Text;
                alert.Accept();
                return t == "Testing?";
            });
        }

        [Test]
        public void TestConfirmCancel() {
            Action f = () => {
                var result = Window.Confirm("Testing?");
                Done(result);
            };
            this.Start(f, wd => {
                var alert = wd.SwitchTo().Alert();
                var t = alert.Text;
                alert.Accept();
                return t == "Testing?";
            });
        }

        [Test]
        public void TestPrompt() {
            Action f = () => {
                var text = Window.Prompt("Testing:");
                Done(text == "Test");
            };
            this.Start(f, wd => {
                var alert = wd.SwitchTo().Alert();
                var t = alert.Text;
                alert.SendKeys("Test");
                alert.Accept();
                return t == "Testing:";
            });
        }

    }

}
