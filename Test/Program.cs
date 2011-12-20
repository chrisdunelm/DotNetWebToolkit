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

        static void Main(string[] args) {

            var t = new TestLoops() { Verbose = true };
            t.Test4ForBreakAndContinue();
            //t.Test3NestedForsWithExtraIfs();
            //t.TestSimplification();
            return;

            //Console.WriteLine();
            //Console.WriteLine("*** DONE ***");
            //Console.ReadKey();

        }
    }
}
