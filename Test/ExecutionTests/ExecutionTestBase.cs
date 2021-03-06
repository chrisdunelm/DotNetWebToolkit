﻿using System;
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
using Test.Categories;
using OpenQA.Selenium;

namespace Test.ExecutionTests {

    [ExecutionTest]
    public abstract class ExecutionTestBase {

        private const int defaultTestIterations = 20;
        private Random rnd = new Random(0);

        public bool Verbose = false;

        class DefaultParamGen : ParamAttribute {

            public override bool GenBool(Random rnd, int iteration) {
                return rnd.Next(2) == 1;
            }

            public override sbyte GenSByte(Random rnd, int iteration) {
                var ret = (sbyte)rnd.Next(0, 255);
                if (ret == sbyte.MinValue) {
                    ret = sbyte.MinValue + 1;
                }
                return ret;
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
                var ret = BitConverter.ToInt64(b, 0);
                if (ret == Int64.MinValue) {
                    ret = Int64.MinValue + 1;
                }
                return ret;
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

        private object[] CreateArgs(MethodInfo methodInfo, int iteration, ParamAttribute testDefaultParamGen) {
            List<object> args = new List<object>();
            var parameters = methodInfo.GetParameters();
            foreach (var arg in parameters) {
                object v;
                var paramGen = (ParamAttribute)arg.GetCustomAttributes(typeof(ParamAttribute), false).FirstOrDefault() ?? testDefaultParamGen;
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
                return "\"" + string.Join("", arg.ToString().Select(x => x >= ' ' && x <= 126 && x != '"' && x != '\\' ? x.ToString() : string.Format("\\u{0:x4}", (int)x))) + "\"";
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

        protected void Test(Delegate d, object knownResult = null, bool knownResultNull = false) {
            var mi = d.Method;
            //var stackTrace = new StackTrace();
            //var testMethod = stackTrace.GetFrame(1).GetMethod().Name;
            //Console.WriteLine("Test++ {0}", testMethod);
            var method = CecilHelper.GetMethod(d);
            var js = Js.CreateFrom(method, this.Verbose, true).Js;
            if (this.Verbose) {
                Console.WriteLine(js);
            }
            var testDefaultParamGen = mi.GetCustomAttribute<ParamAttribute>()
                ?? mi.DeclaringType.GetCustomAttribute<ParamAttribute>()
                ?? defaultParamGen;
            var withinAttr = mi.GetCustomAttribute<WithinAttribute>();
            var withinUlpsAttr = mi.GetCustomAttribute<WithinUlpsAttribute>();
            var withinPercentAttr = mi.GetCustomAttribute<WithinPercentAttribute>();
            var icAttr = mi.GetCustomAttribute<IterationCountAttribute>();
            var minIterations = mi.GetParameters().Max(x =>
                (x.GetCustomAttribute<ParamAttribute>() ?? testDefaultParamGen).MinIterations);
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
            var args = range.Select(i => this.CreateArgs(mi, i, testDefaultParamGen)).ToArray();

            var runResults = range.Select(i => {
                object r = null;
                Exception e = null;
                try {
                    r = d.DynamicInvoke(args[i]);
                } catch (TargetInvocationException ex) {
                    e = ex.InnerException;
                }
                if (knownResult != null || knownResultNull) {
                    Assert.That(r, Is.EqualTo(knownResult));
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
                    var jsArgs = string.Join(", ", arg.Select(x => this.ConvertArgToJavascript(x)));
                    //Console.WriteLine("JS args: '{0}'", jsArgs);
                    var jsCall = @"
var r;
try {
    r = main(" + jsArgs + @");
    console.log(r);
} catch (e) {
    return {exception:[e._.$$TypeNamespace, e._.$$TypeName, e.$$_message]};
}
if (typeof r === 'number') {
    if (isNaN(r)) {
        return 'NaN';
    }
    if (r === Number.POSITIVE_INFINITY) {
        return '+Infinity';
    }
    if (r === Number.NEGATIVE_INFINITY) {
        return '-Infinity';
    }
    return r.toString();
}
return r;
";
                    var jsResult = chrome.ExecuteScript(js + jsCall);
                    if (jsResult != null && jsResult is Dictionary<string, object>) {
                        // Exception
                        Assert.That(runResults[i].Item1, Is.Null, "JS threw exception, but exception not expected");
                        var jsExInfo = ((ICollection<object>)((Dictionary<string, object>)jsResult)["exception"]).Cast<string>().ToArray();
                        var jsExType = jsExInfo[0] + "." + jsExInfo[1];
                        var expectedExType = runResults[i].Item2.GetType().FullName;
                        Assert.That(jsExType, Is.EqualTo(expectedExType));
                    } else {
                        var returnTypeCode = Type.GetTypeCode(d.Method.ReturnType);
                        if (jsResult != null && jsResult.GetType() != d.Method.ReturnType) {
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
                            case TypeCode.Single:
                                switch (jsResult as string) {
                                case "NaN": jsResult = Single.NaN; break;
                                case "+Infinity": jsResult = Single.PositiveInfinity; break;
                                case "-Infinity": jsResult = Single.NegativeInfinity; break;
                                default: jsResult = Single.Parse(jsResult as string); break;
                                }
                                break;
                            case TypeCode.Double:
                                switch (jsResult as string) {
                                case "NaN": jsResult = Double.NaN; break;
                                case "+Infinity": jsResult = Double.PositiveInfinity; break;
                                case "-Infinity": jsResult = Double.NegativeInfinity; break;
                                default: jsResult = Double.Parse(jsResult as string); break;
                                }
                                break;
                            case TypeCode.Char:
                                jsResult = (char)int.Parse(jsResult as string);
                                break;
                            default:
                                jsResult = Convert.ChangeType(jsResult, d.Method.ReturnType);
                                break;
                            }
                        }
                        Assert.That(runResults[i].Item2, Is.Null, "Exception expected in JS, but not thrown");
                        EqualConstraint equalTo = Is.EqualTo(runResults[i].Item1);
                        IResolveConstraint expected = equalTo;
                        if (withinAttr != null) {
                            expected = equalTo.Within(withinAttr.Delta);
                        } else if (withinUlpsAttr != null) {
                            expected = equalTo.Within(withinUlpsAttr.Ulps).Ulps;
                        } else if (withinPercentAttr != null) {
                            expected = equalTo.Within(withinPercentAttr.Percent).Percent;
                        } else {
                            switch (returnTypeCode) {
                            case TypeCode.Single:
                                // Always allow a little inaccuracy with Singles
                                expected = equalTo.Within(0.0001).Percent;
                                break;
                            case TypeCode.Double:
                                expected = equalTo.Within(0.0001).Percent;
                                break;
                            }
                        }
                        Assert.That(jsResult, expected);
                    }
                }
            } finally {
                if (!usingNamespace) {
                    chrome.Quit();
                    chrome.Dispose();
                }
            }

            //Console.WriteLine("Test-- {0}", testMethod);
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
