using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.Cecil;
using Cil2Js.Analysis;
using Cil2Js.Ast;
using Cil2Js.Utils;
using Cil2Js.Output;
using FsCheck.Fluent;
using FsCheck;
using Test.ExecutionTests;
using System.Linq.Expressions;
using Test.Utils;
using Cil2Js;
using Cil2Js.Attributes;

namespace Test {

    class Program {

        static void RunAllTests<T>(bool verbose = false) where T : new() {
            dynamic t = new T();
            t.Verbose = verbose;
            var methods = typeof(T)
                .GetMethods()
                .Where(x => x.GetCustomAttributes(false).Any(y => y.GetType().Name == "TestAttribute"))
                .ToArray();
            foreach (var method in methods) {
                var call = Expression.Call(Expression.Constant(t), method);
                var fn = Expression.Lambda<Action>(call).Compile();
                Console.WriteLine("Calling: {0}();", method.Name);
                fn();
            }
        }

        class A {
            public A(int number) {
                this.i = number;
            }
            private int i;
            public int PlusOne {
                get { return this.i + 1; }
            }
        }

        static int AddOne(int number) {
            var a = new A(number);
            return a.PlusOne;
        }

        static void Main(string[] args) {

            var methodInfo = typeof(Program).GetMethod("AddOne", BindingFlags.NonPublic | BindingFlags.Static);
            string javaScript = Js.CreateFrom(methodInfo);
            Console.WriteLine(javaScript);

            //var t = new TestSimpleClasses() { Verbose = true };
            //t.TestVirtualComplex();
            //return;

            //var mi = typeof(Program).GetMethod("T0");
            //var method  = Transcoder.GetMethod(mi);
            ////var js = Js.CreateFrom(method);
            ////Console.WriteLine(js);

            //var ast = Transcoder.ToAst(method, true);
            //var show = ShowVisitor.V(method, ast);
            //Console.WriteLine(show);
            //var js = JsMethod.Create(method, "T0", null, ast);
            //Console.WriteLine(js);

            //Console.WriteLine();
            //Console.WriteLine("*** DONE ***");
            //Console.ReadKey();

        }
    }
}
