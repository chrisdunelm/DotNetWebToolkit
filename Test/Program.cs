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
using Cil2Js.Web;

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

        public static void T0() {
            var canvas = (HtmlCanvasElement)Document.GetElementById("canvasId");
            var ctx = (CanvasRenderingContext2D)canvas.GetContext(CanvasContext.TwoD);
            string fill1 = "#ff0000";
            string fill2 = "#0000ff";
            bool f1 = true;
            Window.SetInterval(() => {
                ctx.FillStyle = f1 ? fill1 : fill2;
                f1 = !f1;
                ctx.FillRect(50, 50, 100, 50);
            }, 5000);
        }

        class C1<T> {
            public C1(T i) { this.Value = i; }
            public T Value { get; set; }
        }

        public static int T1() {
            var c = new C1<int>(2);
            return c.Value;
        }

        public static int T2() {
            var c = new C1<int>(2);
            c.Value = 3;
            return 0;
        }

        class C2<T> {
            public T t;
        }

        public static int T3() {
            var c = new C2<int>();
            c.t = 5;
            return c.t;
        }

        static void Main(string[] args) {
            //var mi = typeof(Program).GetMethod("T3");
            //var js = Transcoder.ToJs(mi, true);
            //Console.WriteLine(js);

            var t = new TestGenericVirtualCalls() { Verbose = true };
            t.TestGenericMethodVCallMultipleInstantiations();
            return;

            //var t = new TestCecilExtensions();
            //t.TestMethodBasesV2();

            //var location = Assembly.GetExecutingAssembly().Location;
            //var module = ModuleDefinition.ReadModule(location);
            //var typeD = (TypeReference)module.GetType("Test.D");
            //var dm = typeD.GetMethods().ToArray();
            //var c = typeD.GetBaseType();
            //var cm = c.GetMethods().ToArray();
            //var b = c.GetBaseType();
            //var bm = b.GetMethods().ToArray();
            //var a = b.GetBaseType();
            //var am = a.GetMethods().ToArray();
            //var o = a.GetBaseType();
            //var z = o.GetBaseType();

            //var dv = ((TypeDefinition)typeD).Methods.First(x => x.Name == "V");
            //var cv = dv.GetBaseMethodByType();
            //var bv = cv.GetBaseMethodByType();
            //var av = bv.GetBaseMethodByType();

            //var typeZ = (TypeReference)module.GetType("Test.Z");
            //var m = typeZ.Resolve().Methods.First(x => x.Name == "M");
            //var m2 = new GenericInstanceMethod(m);
            //m2.GenericArguments.Add(module.TypeSystem.Int32);

            //Js.CreateFrom(m2, true);

            //Console.WriteLine();
        }

    }

    class Z {
        public T M<T>(T a) {
            return a;
        }
    }

    class A<T, U> {
        public virtual T V(T a) {
            return a;
        }
    }

    class B<S, T> : A<T, S> {
        public override T V(T a) {
            return a;
        }
    }

    class C<R> : B<int, R> {
        public override R V(R a) {
            return a;
        }
        public Z M<Z>(Z z) {
            return z;
        }
    }

    class D : C<bool> {
        public override bool V(bool a) {
            return !a;
        }
    }

}
