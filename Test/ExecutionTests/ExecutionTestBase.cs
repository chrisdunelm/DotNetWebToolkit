using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Reflection;
using NUnit.Framework;
using Cil2Js;
using Cil2Js.Utils;
using Cil2Js.Output;
using Test.Utils;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using System.Threading;

namespace Test.ExecutionTests {
    public class ExecutionTestBase {

        private int testIterations = 20;
        private Random rnd = new Random(0);

        public bool Verbose = false;

        private object[] CreateArgs(MethodDefinition method) {
            List<object> args = new List<object>();
            foreach (var arg in method.Parameters) {
                object v;
                switch (arg.ParameterType.FullName) {
                case "System.Boolean":
                    v = this.rnd.Next(2) == 1;
                    break;
                case "System.Int32":
                    v = this.rnd.Next(0, 100);
                    break;
                case "System.Double":
                    v = this.rnd.NextDouble() * 100.0;
                    break;
                case "System.String":
                    int length = this.rnd.Next(10);
                    string s = "";
                    for (int i = 0; i < length; i++) {
                        s += (char)(65 + this.rnd.Next(26));
                    }
                    v = s;
                    break;
                default:
                    throw new NotImplementedException("Cannot handle: " + arg.ParameterType.FullName);
                }
                args.Add(v);
            }
            return args.ToArray();
        }

        private string ConvertArgToJavascript(object arg) {
            if (arg == null) {
                return "null";
            }
            var tc = Type.GetTypeCode(arg.GetType());
            switch (tc) {
            case TypeCode.Boolean:
                return (bool)arg ? "true" : "false";
            case TypeCode.Int32:
            case TypeCode.Double:
                return arg.ToString();
            case TypeCode.String:
                return "\"" + arg.ToString() + "\"";
            default:
                throw new NotImplementedException("Cannot convert: " + tc);
            }
        }

        protected void Test(params Delegate[] ds) {
            foreach (var d in ds) {
                Test(d);
            }
        }

        protected void Test(Delegate d) {
            var mi = d.Method;
            var method = CecilHelper.GetMethod(d);
            var js = Js.CreateFrom(method, this.Verbose);
            if (this.Verbose) {
                Console.WriteLine(js);
            }
            var iterationCount = method.Parameters.Any() ? this.testIterations : 1;
            var range = Enumerable.Range(0, iterationCount);
            var args = range.Select(i => this.CreateArgs(method)).ToArray();

            var runResults = range.Select(i => {
                object r = null;
                Exception e = null;
                try {
                    r = d.DynamicInvoke(args[i]);
                } catch (TargetInvocationException ex) {
                    e = ex.InnerException;
                }
                return Tuple.Create(r, e);
            }).ToArray();

            using (var chrome = NamespaceSetup.ChromeService != null ?
                new RemoteWebDriver(NamespaceSetup.ChromeService.ServiceUrl, DesiredCapabilities.Chrome()) :
                new ChromeDriver()) {
                    Thread.Sleep(100);
                    try {
                        for (int i = 0; i < args.Length; i++) {
                            var arg = args[i];
                            var jsCall = string.Format("return main({0});", string.Join(", ", arg.Select(x => this.ConvertArgToJavascript(x))));
                            var jsResult = chrome.ExecuteScript(js + jsCall);
                            if (jsResult != null && jsResult.GetType() != d.Method.ReturnType) {
                                jsResult = Convert.ChangeType(jsResult, d.Method.ReturnType);
                            }
                            Assert.That(jsResult, Is.EqualTo(runResults[i].Item1).Within(0.00001));
                        }
                    } finally {
                        Thread.Sleep(100);
                        chrome.Quit();
                    }
            }

        }

        //protected void Test(Delegate d) {
        //    var mi = d.Method;
        //    var method = CecilHelper.GetMethod(d);
        //    var js = Js.CreateFrom(method, this.Verbose);
        //    //var ast = Transcoder.ToAst(method, this.Verbose);
        //    // Test that ShowVisitor works
        //    //var show = ShowVisitor.V(method, ast);
        //    //Assert.That(show, Is.Not.Null);
        //    // Test that AST is correct and Js creator works
        //    //var js = JsMethod.Create(method, "test", null, ast);
        //    if (this.Verbose) {
        //        //Console.WriteLine(show);
        //        Console.WriteLine(js);
        //    }
        //    var iterationCount = method.Parameters.Any() ? this.testIterations : 1;
        //    var range = Enumerable.Range(0, iterationCount);
        //    var args = range.Select(i => this.CreateArgs(method)).ToArray();
        //    var runResults = range.Select(i => {
        //        object r = null;
        //        Exception e = null;
        //        try {
        //            r = d.DynamicInvoke(args[i]);
        //        } catch (TargetInvocationException ex) {
        //            e = ex.InnerException;
        //        }
        //        return Tuple.Create(r, e);
        //    }).ToArray();
        //    var jsResults = JsRunner.Run(js, "main", args, mi.ReturnType);
        //    for (int i = 0; i < iterationCount; i++) {
        //        var jsResult = jsResults[i];
        //        Assert.That(jsResult, Is.EqualTo(runResults[i]));
        //    }
        //}

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
