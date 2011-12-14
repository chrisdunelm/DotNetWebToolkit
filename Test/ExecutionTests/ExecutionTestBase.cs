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

        protected void Test(params Delegate[] ds) {
            foreach (var d in ds) {
                Test(d);
            }
        }

        protected void Test(Delegate d) {
            var mi = d.Method;
            var fn = Assembly.GetExecutingAssembly().Location;
            var module = ModuleDefinition.ReadModule(fn);
            var type = module.GetType(mi.DeclaringType.FullName);
            var method = type.Methods.First(x => x.Name == mi.Name);
            var js = Js.CreateFrom(method, this.Verbose);
            //var ast = Transcoder.ToAst(method, this.Verbose);
            // Test that ShowVisitor works
            //var show = ShowVisitor.V(method, ast);
            //Assert.That(show, Is.Not.Null);
            // Test that AST is correct and Js creator works
            //var js = JsMethod.Create(method, "test", null, ast);
            if (this.Verbose) {
                //Console.WriteLine(show);
                Console.WriteLine(js);
            }
            var iterationCount = method.Parameters.Any() ? this.testIterations : 1;
            var range = Enumerable.Range(0, iterationCount);
            var args = range.Select(i => this.CreateArgs(method)).ToArray();
            var runResults = range.Select(i => mi.Invoke(d.Target, args[i])).ToArray();
            var jsResults = JsRunner.Run(js, "main", args, mi.ReturnType);
            for (int i = 0; i < iterationCount; i++) {
                var jsResult = jsResults[i];
                Assert.That(jsResult, Is.EqualTo(runResults[i]));
            }
        }

    }
}
