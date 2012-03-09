using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Output;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Test.Categories;
using Test.Utils;

namespace Test.BrowserTests {

    [BrowserTest]
    public abstract class BrowserTestBase {

        public bool Verbose = false;

        protected string HtmlBody { get; set; }

        private string MakeHtml(string js) {
            string html = @"
<!doctype html>
<html>
    <head>
        <title>Test Running</title>
        <script type=""text/javascript"">
" + js + @"
window.onload = function() {
    try {
        main();
    } catch (e) { }
};
        </script>
    </head>
    <body>" + (this.HtmlBody ?? "") + @"</body>
</html>";
            return html;
        }

        protected void Start(Action action, TimeSpan? timeout = null) {
            if (timeout == null) {
                timeout = TimeSpan.FromSeconds(1);
            }
            var method = CecilHelper.GetMethod(action.Method);
            var js = Js.CreateFrom(method, this.Verbose, true);
            if (this.Verbose) {
                Console.WriteLine(js);
            }
            var completeHtml = this.MakeHtml(js);
            using (var http = new HttpListener()) {
                http.Prefixes.Add("http://localhost:7890/");
                http.Start();
                IAsyncResult getContextAsync = null;
                AsyncCallback cb = null;
                cb = state => {
                    var context = http.EndGetContext(getContextAsync);
                    var response = context.Response;
                    var output = response.OutputStream;
                    var bHtml = Encoding.UTF8.GetBytes(completeHtml);
                    output.Write(bHtml, 0, bHtml.Length);
                    output.Close();
                    //getContextAsync = http.BeginGetContext(cb, null);
                };
                getContextAsync = http.BeginGetContext(cb, null);
                //var usingNamespace = NamespaceSetup.Chrome != null;
                //var chrome = usingNamespace ? NamespaceSetup.Chrome : new ChromeDriver();
                using (var chrome = NamespaceSetup.ChromeService != null ?
                    new RemoteWebDriver(NamespaceSetup.ChromeService.ServiceUrl, DesiredCapabilities.Chrome()) :
                    new ChromeDriver()) {
                    try {
                        chrome.Manage().Timeouts().ImplicitlyWait(timeout.Value);
                        chrome.Url = "http://localhost:7890/";
                        bool isPass;
                        try {
                            var done = chrome.FindElementById("__done__");
                            isPass = done.Text == "pass";
                        } catch (NoSuchElementException) {
                            isPass = false;
                        } catch {
                            isPass = false;
                        }
                        Assert.That(isPass, Is.True);
                    } finally {
                        //if (!usingNamespace) {
                            chrome.Quit();
                            chrome.Dispose();
                        //}
                    }
                }
                http.Stop();
            }
        }

        [Js(@"
var _done = document.createElement('div');
_done.setAttribute('id', '__done__');
_done.innerHTML = a;
document.body.appendChild(_done);
throw {};
")]
        private static void SetDoneJs(string text) {
            throw new Exception();
        }

        private static bool resultSet = false;

        private static void SetDone(string text) {
            if (resultSet) {
                return;
            }
            resultSet = true;
            SetDoneJs(text);
        }

        protected static void Pass() {
            SetDone("pass");
        }

        protected static void Fail() {
            SetDone("fail");
        }

        protected static void Done(bool isPass) {
            if (isPass) {
                Pass();
            } else {
                Fail();
            }
        }

    }

    [SetUpFixture]
    public class NamespaceSetup {

        public static ChromeDriverService ChromeService;
        //public static RemoteWebDriver Chrome;

        [SetUp]
        public void Setup() {
            ChromeService = ChromeDriverService.CreateDefaultService();
            ChromeService.Start();
            //Chrome = new RemoteWebDriver(ChromeService.ServiceUrl, DesiredCapabilities.Chrome());
        }

        [TearDown]
        public void Teardown() {
            //Chrome.Quit();
            //Chrome.Dispose();
            //Chrome = null;
            ChromeService.Dispose();
            ChromeService = null;
        }

    }

}
