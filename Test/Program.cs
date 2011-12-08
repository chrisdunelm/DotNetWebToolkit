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

        public static string T0(string a) {
            if (a == "abc") {
                return "abcd";
            } else {
                return a;
            }
        }

        public static int T1(int a) {
            if (a >= 0) {
                return T2();
            } else {
                return T2() + 2;
            }
        }

        public static int T2() {
            return 5;
        }

        static void Main(string[] args) {

            //var t = new TestLogic() { Verbose = true };
            //t.Test1IfInt();

            var mi = typeof(Program).GetMethod("T1");
            var method  = Transcoder.GetMethod(mi);
            var js = Js.CreateFrom(method);
            Console.WriteLine(js);

            Console.WriteLine();
            Console.WriteLine("*** DONE ***");
            Console.ReadKey();

        }
    }
}
