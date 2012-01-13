﻿using System;
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

        static void Main(string[] args) {
            //var mi = typeof(Program).GetMethod("T0");
            //var js = Transcoder.ToJs(mi, true);
            //Console.WriteLine(js);

            var t = new TestArrays() { Verbose = true };
            t.TestEnumerator();
            return;

            //var t = new TestCecilExtensions();
            //t.TestMethodBasesV2();
        }

    }

}
