using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.Reflection;
using NUnit.Framework;
using DotNetWebToolkit;
using DotNetWebToolkit.Cil2Js.Utils;
using DotNetWebToolkit.Cil2Js.Output;
using Test.Utils;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using System.Threading;
using NUnit.Framework.Constraints;
using System.Diagnostics;

namespace Test.ExecutionTests {
    public abstract class ExecutionTestBase {

        private const int defaultTestIterations = 20;
        private Random rnd = new Random(0);

        public bool Verbose = false;

        class DefaultParamGen : ParamAttribute {

            public override bool GenBool(Random rnd, int iteration) {
                return rnd.Next(2) == 1;
            }

            public override sbyte GenSByte(Random rnd, int iteration) {
                return (sbyte)rnd.Next(0, 256);
            }

            public override byte GenByte(Random rnd, int iteration) {
                return (byte)rnd.Next(0, 256);
            }

            public override short GenInt16(Random rnd, int iteration) {
                return (short)rnd.Next(0, 100);
            }

            public override int GenInt32(Random rnd, int iteration) {
                return rnd.Next(0, 100);
            }

            public override long GenInt64(Random rnd, int iteration) {
                var b = new byte[8];
                rnd.NextBytes(b);
                return BitConverter.ToInt64(b, 0);
            }

            public override ushort GenUInt16(Random rnd, int iteration) {
                return (ushort)rnd.Next(0, 100);
            }

            public override uint GenUInt32(Random rnd, int iteration) {
                return (uint)rnd.Next(0, 100);
            }

            public override ulong GenUInt64(Random rnd, int iteration) {
                var b = new byte[8];
                rnd.NextBytes(b);
                return BitConverter.ToUInt64(b, 0);
            }

            public override float GenSingle(Random rnd, int iteration) {
                return (float)(rnd.NextDouble() * 100.0);
            }

            public override double GenDouble(Random rnd, int iteration) {
                return rnd.NextDouble() * 100.0;
            }

            public override string GenString(Random rnd, int iteration) {
                int length = rnd.Next(10);
                string s = "";
                for (int i = 0; i < length; i++) {
                    s += (char)(65 + rnd.Next(26));
                }
                return s;
            }

            public override char GenChar(Random rnd, int iteration) {
                var v = rnd.Next(32, 0x7fff);
                return (char)v;
            }

        }

        private readonly DefaultParamGen defaultParamGen = new DefaultParamGen();

        private object[] CreateArgs(MethodInfo methodInfo, int iteration) {
            List<object> args = new List<object>();
            var parameters = methodInfo.GetParameters();
            foreach (var arg in parameters) {
                object v;
                var paramGen = (ParamAttribute)arg.GetCustomAttributes(typeof(ParamAttribute), false).FirstOrDefault() ?? defaultParamGen;
                var typeCode = Type.GetTypeCode(arg.ParameterType);
                switch (typeCode) {
                case TypeCode.Boolean:
                    v = paramGen.GenBool(this.rnd, iteration);
                    break;
                case TypeCode.SByte:
                    v = paramGen.GenSByte(this.rnd, iteration);
                    break;
                case TypeCode.Byte:
                    v = paramGen.GenByte(this.rnd, iteration);
                    break;
                case TypeCode.Int16:
                    v = paramGen.GenInt16(this.rnd, iteration);
                    break;
                case TypeCode.Int32:
                    v = paramGen.GenInt32(this.rnd, iteration);
                    break;
                case TypeCode.Int64:
                    v = paramGen.GenInt64(this.rnd, iteration);
                    break;
                case TypeCode.UInt16:
                    v = paramGen.GenUInt16(this.rnd, iteration);
                    break;
                case TypeCode.UInt32:
                    v = paramGen.GenUInt32(this.rnd, iteration);
                    break;
                case TypeCode.UInt64:
                    v = paramGen.GenUInt64(this.rnd, iteration);
                    break;
                case TypeCode.Single:
                    v = paramGen.GenSingle(this.rnd, iteration);
                    break;
                case TypeCode.Double:
                    v = paramGen.GenDouble(this.rnd, iteration);
                    break;
                case TypeCode.String:
                    v = paramGen.GenString(this.rnd, iteration);
                    break;
                case TypeCode.Char:
                    v = paramGen.GenChar(this.rnd, iteration);
                    break;
                default:
                    throw new NotImplementedException("Cannot handle: " + typeCode);
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
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
                return arg.ToString();
            case TypeCode.Single:
            case TypeCode.Double:
                return ((IFormattable)arg).ToString("r", null);
            case TypeCode.Int64:
                var int64 = (UInt64)(Int64)arg;
                return string.Format("[{0},{1}]", int64 >> 32, int64 & 0xffffffff);
            case TypeCode.UInt64:
                var uint64 = (UInt64)arg;
                return string.Format("[{0},{1}]", uint64 >> 32, uint64 & 0xffffffff);
            case TypeCode.String:
                return "\"" + arg.ToString() + "\"";
            case TypeCode.Char:
                return ((int)(char)arg).ToString();
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
            var stackTrace = new StackTrace();
            var testMethod = stackTrace.GetFrame(1).GetMethod().Name;
            Console.WriteLine("Test++ {0}", testMethod);
            var method = CecilHelper.GetMethod(d);
            var js = Js.CreateFrom(method, this.Verbose, true);
            if (this.Verbose) {
                Console.WriteLine(js);
            }
            var withinAttr = mi.GetCustomAttribute<WithinAttribute>();
            var withinUlpsAttr = mi.GetCustomAttribute<WithinUlpsAttribute>();
            var withinPercentAttr = mi.GetCustomAttribute<WithinPercentAttribute>();
            var icAttr = mi.GetCustomAttribute<IterationCountAttribute>();
            var minIterations = mi.GetParameters().Max(x => x.GetCustomAttribute<ParamAttribute>().NullThru(y => y.MinIterations));
            int iterationCount;
            if (icAttr != null) {
                iterationCount = icAttr.IterationCount;
            } else {
                iterationCount = method.Parameters.Any() ? defaultTestIterations : 1;
            }
            if (iterationCount < minIterations) {
                iterationCount = minIterations.Value;
            }
            var range = Enumerable.Range(0, iterationCount);
            var args = range.Select(i => this.CreateArgs(mi, i)).ToArray();

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

            var usingNamespace = NamespaceSetup.Chrome != null;
            var chrome = usingNamespace ? NamespaceSetup.Chrome : new ChromeDriver();
            try {
                for (int i = 0; i < args.Length; i++) {
                    var arg = args[i];
                    if (!mi.IsStatic) {
                        arg = arg.Prepend(null).ToArray();
                    }
                    var jsCall = string.Format("return main({0});", string.Join(", ", arg.Select(x => this.ConvertArgToJavascript(x))));
                    var jsResult = chrome.ExecuteScript(js + jsCall);
                    if (jsResult != null && jsResult.GetType() != d.Method.ReturnType) {
                        var returnTypeCode = Type.GetTypeCode(d.Method.ReturnType);
                        switch (returnTypeCode) {
                        case TypeCode.Int64: {
                                var array = (IList<object>)jsResult;
                                var hi = Convert.ToUInt64(array[0]);
                                var lo = Convert.ToUInt64(array[1]);
                                jsResult = (long)(((ulong)hi) << 32 | (ulong)lo);
                            }
                            break;
                        case TypeCode.UInt64: {
                                var array = (IList<object>)jsResult;
                                var hi = Convert.ToUInt64(array[0]);
                                var lo = Convert.ToUInt64(array[1]);
                                jsResult = ((ulong)hi) << 32 | (ulong)lo;
                            }
                            break;
                        default:
                            jsResult = Convert.ChangeType(jsResult, d.Method.ReturnType);
                            break;
                        }
                    }
                    EqualConstraint equalTo = Is.EqualTo(runResults[i].Item1);
                    IResolveConstraint expected = equalTo;
                    if (withinAttr != null) {
                        expected = equalTo.Within(withinAttr.Delta);
                    }
                    if (withinUlpsAttr != null) {
                        expected = equalTo.Within(withinUlpsAttr.Ulps).Ulps;
                    }
                    if (withinPercentAttr != null) {
                        expected = equalTo.Within(withinPercentAttr.Percent).Percent;
                    }
                    Assert.That(jsResult, expected);
                }
            } finally {
                if (!usingNamespace) {
                    chrome.Quit();
                    chrome.Dispose();
                }
            }

            Console.WriteLine("Test-- {0}", testMethod);
        }

    }

    [SetUpFixture]
    public class NamespaceSetup {

        private static ChromeDriverService ChromeService;
        public static RemoteWebDriver Chrome;

        [SetUp]
        public void Setup() {
            ChromeService = ChromeDriverService.CreateDefaultService();
            ChromeService.Start();
            Chrome = new RemoteWebDriver(ChromeService.ServiceUrl, DesiredCapabilities.Chrome());
        }

        [TearDown]
        public void Teardown() {
            Chrome.Quit();
            Chrome.Dispose();
            Chrome = null;
            ChromeService.Dispose();
            ChromeService = null;
        }

    }

}
