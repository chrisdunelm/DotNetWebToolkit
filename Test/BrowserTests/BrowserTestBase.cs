using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Output;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Test.Utils;

namespace Test.BrowserTests {

    public class CDriver : ChromeDriver {
    }

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
    main();
};
        </script>
    </head>
    <body id=""body"">" + (this.HtmlBody ?? "") + @"</body>
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
                getContextAsync = http.BeginGetContext(state => {
                    var context = http.EndGetContext(getContextAsync);
                    var response = context.Response;
                    var output = response.OutputStream;
                    var bHtml = Encoding.UTF8.GetBytes(completeHtml);
                    output.Write(bHtml, 0, bHtml.Length);
                    output.Close();
                }, null);
                using (var chrome = NamespaceSetup.ChromeService != null ?
                    new RemoteWebDriver(NamespaceSetup.ChromeService.ServiceUrl, DesiredCapabilities.Chrome()) :
                    new ChromeDriver()) {
                    try {
                        chrome.Url = "http://localhost:7890/";
                        chrome.Manage().Timeouts().ImplicitlyWait(timeout.Value);
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
                        chrome.Quit();
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

        [SetUp]
        public void Setup() {
            ChromeService = ChromeDriverService.CreateDefaultService();
            ChromeService.Start();

        }

        [TearDown]
        public void Teardown() {
            ChromeService.Dispose();
            ChromeService = null;
        }

    }

}
