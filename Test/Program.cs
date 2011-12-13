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

    abstract class C0 {
        public abstract void A();
    }

    class C1 : C0 {
        public override void A() {
            throw new NotImplementedException();
        }
    }

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

        //class C1 {

        //    private int i = 3;

        //    public int Get() {
        //        return this.i;
        //    }
        //}

        //public static int T0(string a) {
        //    var c = new C1();
        //    return c.Get();
        //}

        //[Export("EntryPoint")]
        //public static string T1(string s) {
        //    return s.ToString();
        //}

        private int i = 5;
        public int T0(int a) {
            return this.i;
        }

        static void Main(string[] args) {
            var t = new TestSimpleClasses() { Verbose = true };
            t.TestVirtualComplex();
            return;

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
