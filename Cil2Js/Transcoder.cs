using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Cil2Js.Ast;
using Cil2Js.Analysis;
using Cil2Js.Output;
using System.Reflection;

namespace Cil2Js {
    public static class Transcoder {

        public static ICode ToAst(MethodDefinition method, bool verbose = false) {
            var astGen = new CreateAst(method);
            var ast = astGen.Create();
            var finalAst = FlowAnalyser.Analyse(method, ast, verbose);
            return finalAst;
        }

        public static string ToJs(MethodDefinition method, string jsMethodName, JsMethod.Resolver resolver, bool verbose = false) {
            var ast = ToAst(method, verbose);
            return JsMethod.Create(method, resolver, ast);
        }

        public static string ToJs(MethodInfo methodInfo, string jsMethodName, JsMethod.Resolver resolver, bool verbose = false) {
            var method = GetMethod(methodInfo);
            return ToJs(method, jsMethodName, resolver, verbose);
        }

        public static MethodDefinition GetMethod(MethodInfo mi) {
            // TODO: This won't handle overloaded methods (arguments and generics)
            var filename = mi.DeclaringType.Assembly.Location;
            var module = ModuleDefinition.ReadModule(filename);
            var type = module.GetType(mi.DeclaringType.FullName.Replace('+', '.'));
            var method = type.Methods.First(x => x.Name == mi.Name);
            return method;
        }

    }
}
