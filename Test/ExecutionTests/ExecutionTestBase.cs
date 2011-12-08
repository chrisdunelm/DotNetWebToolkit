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

        private int testIterations = 10;
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
                default:
                    throw new NotImplementedException("Cannot handle: " + arg.ParameterType.FullName);
                }
                args.Add(v);
            }
            return args.ToArray();
        }

        protected void Test(Delegate d) {
            var mi = d.Method;
            var fn = Assembly.GetExecutingAssembly().Location;
            var module = ModuleDefinition.ReadModule(fn);
            var type = module.GetType(mi.DeclaringType.FullName);
            var method = type.Methods.First(x => x.Name == mi.Name);
            var ast = Transcoder.ToAst(method, this.Verbose);
            // Test that ShowVisitor works
            var show = ShowVisitor.V(method, ast);
            Assert.That(show, Is.Not.Null);
            // Test that AST is correct and Js creator works
            var js = Js.Create(method, "test", ast);
            if (this.Verbose) {
                Console.WriteLine(show);
                Console.WriteLine(js);
            }
            var range = Enumerable.Range(0, this.testIterations);
            var args = range.Select(i => this.CreateArgs(method)).ToArray();
            var runResults = range.Select(i => mi.Invoke(d.Target, args[i])).ToArray();
            var jsResults = JsRunner.Run(js, "test", args);
            for (int i = 0; i < this.testIterations; i++) {
                var jsResult = jsResults[i];
                if (jsResult.GetType() != mi.ReturnType) {
                    // Some returns will require casting - e.g. booleans will be returned as integers
                    jsResult = Convert.ChangeType(jsResult, mi.ReturnType);
                }
                Assert.That(jsResult, Is.EqualTo(runResults[i]));
            }
        }

    }
}
