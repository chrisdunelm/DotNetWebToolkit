using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Test.BrowserTests {

    [TestFixture]
    public class TestEvents : BrowserTestBase {

        [Test]
        public void TestImageOnLoad() {
            Action f = () => {
                var img = new ImageElement();
                img.OnLoad = e => {
                    Fail();
                };
                img.OnLoad = e => {
                    Window.SetTimeout(() => Pass(), 50);
                };
                img.Src = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
            };
            this.Start(f);
        }

        [Test]
        public void TestButtonOnClick() {
            Action f = () => {
                Window.Document.GetElementById("b").OnClick = e => Pass();
            };
            this.HtmlBody = "<button id=\"b\"></button>";
            this.Start(f, wd => {
                wd.FindElement(By.Id("b")).Click();
            });
        }

        [Test]
        public void TestTextInputOnChange() {
            Action f = () => {
                Window.Document.GetElementById("i").OnChange = e => Pass();
            };
            this.HtmlBody = "<input id=\"i\" type=\"text\" />";
            this.Start(f, wd => {
                wd.FindElement(By.Id("i")).SendKeys("Testing\n");
            });
        }

        [Test]
        public void TestTextInputOnKeyDown() {
            Action f = () => {
                Window.Document.GetElementById("i").OnKeyDown = e => Pass();
            };
            this.HtmlBody = "<input id=\"i\" type=\"text\" />";
            this.Start(f, wd => {
                wd.FindElement(By.Id("i")).SendKeys("1");
            });
        }

        [Test]
        public void TestTextInputOnKeyPress() {
            Action f = () => {
                Window.Document.GetElementById("i").OnKeyPress = e => Pass();
            };
            this.HtmlBody = "<input id=\"i\" type=\"text\" />";
            this.Start(f, wd => {
                wd.FindElement(By.Id("i")).SendKeys("1");
            });
        }

        [Test]
        public void TestTextInputOnKeyUp() {
            Action f = () => {
                Window.Document.GetElementById("i").OnKeyUp = e => Pass();
            };
            this.HtmlBody = "<input id=\"i\" type=\"text\" />";
            this.Start(f, wd => {
                wd.FindElement(By.Id("i")).SendKeys("1");
            });
        }

    }

}
