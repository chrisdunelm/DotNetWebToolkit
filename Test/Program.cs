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

        static int T0(int a, int b) {
            if (a == b) {
                b++;
            }
            return b;
        }

        static void Main(string[] args) {

            MethodInfo methodInfo = typeof(Program).GetMethod("T0", BindingFlags.NonPublic|BindingFlags.Static);
            MethodDefinition method = Transcoder.GetMethod(methodInfo);
            ICode ast = Transcoder.ToAst(method, true);
            var show = ShowVisitor.V(method, ast);
            Console.WriteLine(show);
            return;

            //RunAllTests<TestLoops>(true);

            var t = new TestLogic() { Verbose = true };
            t.Test1IfInt();

            Console.WriteLine();
            Console.WriteLine("*** DONE ***");
            Console.ReadKey();

        }
    }
}
