using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.JsResolvers;
using DotNetWebToolkit.Cil2Js.Output;
using DotNetWebToolkit.Server;
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

        private Dictionary<string, Func<byte[], string>> urls = new Dictionary<string, Func<byte[], string>>();
        private JsonTypeMap jsonTypeMap = null;

        protected Json GetJson() {
            return new Json(this.jsonTypeMap);
        }

        protected void SetUrl(string url, Func<byte[], string> response) {
            this.urls[url] = response;
        }

        protected void SetUrlJson(string url, Func<byte[], object> jsonResponse) {
            this.urls[url] = data => {
                if (this.jsonTypeMap == null) {
                    throw new InvalidOperationException("Cannot use JSON, jsonTypeMap not set");
                }
                var toJson = jsonResponse(data);
                var jsonCoder = this.GetJson();
                var ret = jsonCoder.Encode(toJson);
                return ret;
            };
        }

        protected void SetUrlJson(string url, Action<byte[]> jsonResponse) {
            this.SetUrlJson(url, data => {
                jsonResponse(data);
                return null;
            });
        }

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

        protected void Start(Action action, Action<IWebDriver> preExtraTest, TimeSpan? timeout = null) {
            this.Start(action, wd => {
                preExtraTest(wd);
                return true;
            }, null, timeout);
        }

        protected void Start(Action action, Func<IWebDriver, bool> preExtraTest = null, Func<IWebDriver, bool> postExtraTest = null, TimeSpan? timeout = null) {
            if (timeout == null) {
                timeout = TimeSpan.FromSeconds(1000);
            }
            var method = CecilHelper.GetMethod(action.Method);
            var jsResult = Js.CreateFrom(method, this.Verbose, true);
            var js = jsResult.Js;
            if (this.Verbose) {
                Console.WriteLine(js);
            }
            var completeHtml = this.MakeHtml(js);
            using (var http = new HttpListener()) {
                http.Prefixes.Add("http://localhost:7890/");
                http.Start();
                AsyncCallback cb = null;
                cb = asyncState => {
                    if (!http.IsListening) {
                        return;
                    }
                    HttpListenerContext context;
                    try {
                        context = http.EndGetContext(asyncState);
                    } catch (HttpListenerException) {
                        return;
                    } catch (ObjectDisposedException) {
                        return;
                    }
                    using (var response = context.Response) {
                        var output = response.OutputStream;
                        var path = context.Request.Url.AbsolutePath;
                        string responseString;
                        if (path == "/") {
                            responseString = completeHtml;
                        } else {
                            var request = context.Request;
                            using (var ms = new MemoryStream()) {
                                request.InputStream.CopyTo(ms);
                                var bytes = ms.ToArray();
                                Func<byte[], string> fn;
                                urls.TryGetValue(path, out fn);
                                responseString = fn != null ? fn(bytes) : "";
                            }
                        }
                        var bHtml = Encoding.UTF8.GetBytes(responseString);
                        output.Write(bHtml, 0, bHtml.Length);
                    }
                    http.BeginGetContext(cb, null);
                };
                http.BeginGetContext(cb, null);
                //var usingNamespace = NamespaceSetup.Chrome != null;
                //var chrome = usingNamespace ? NamespaceSetup.Chrome : new ChromeDriver();
                using (var chrome = NamespaceSetup.ChromeService != null ?
                    new RemoteWebDriver(NamespaceSetup.ChromeService.ServiceUrl, DesiredCapabilities.Chrome()) :
                    new ChromeDriver()) {
                    try {
                        this.jsonTypeMap = jsResult.TypeMap;
                        chrome.Manage().Timeouts().ImplicitlyWait(timeout.Value);
                        chrome.Url = "http://localhost:7890/";
                        bool isPass = true;
                        if (preExtraTest != null) {
                            isPass = preExtraTest(chrome);
                        }
                        if (isPass) {
                            try {
                                var done = chrome.FindElementById("__done__");
                                isPass = done.Text == "pass";
                                if (isPass && postExtraTest != null) {
                                    isPass = postExtraTest(chrome);
                                }
                            } catch (NoSuchElementException) {
                                isPass = false;
                            } catch {
                                isPass = false;
                            }
                        }
                        Assert.That(isPass, Is.True);
                    } finally {
                        //if (!usingNamespace) {
                        chrome.Quit();
                        chrome.Dispose();
                        //}
                        this.jsonTypeMap = null;
                    }
                }
                http.Abort();
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
