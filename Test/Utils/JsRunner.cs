using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.JScript;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.JScript.Vsa;

namespace Test.Utils {
    class JsRunner : MarshalByRefObject {

        public static object[] Run(string js, string functionName, object[][] args, Type returnType) {
            var info = new AppDomainSetup {
                ApplicationBase = Path.GetDirectoryName(typeof(JsRunner).Assembly.Location)
            };
            var jsDomain = AppDomain.CreateDomain("JsRunner", null, info);
            var jsRunner = (JsRunner)jsDomain.CreateInstanceAndUnwrap(typeof(JsRunner).Assembly.FullName, typeof(JsRunner).FullName);
            var ret = jsRunner.CompileAndExecute(js, functionName, args, returnType);
            AppDomain.Unload(jsDomain);
            return ret;
        }

#pragma warning disable 618
        class Site : IJSVsaSite {
            public void GetCompiledState(out byte[] pe, out byte[] debugInfo) {
                throw new NotImplementedException();
            }

            public object GetEventSourceInstance(string itemName, string eventSourceName) {
                throw new NotImplementedException();
            }

            public object GetGlobalInstance(string name) {
                throw new NotImplementedException();
            }

            public void Notify(string notify, object info) {
                throw new NotImplementedException();
            }

            public bool OnCompilerError(IJSVsaError error) {
                throw new NotImplementedException();
            }
        }
#pragma warning restore 618

        private object[] CompileAndExecute(string js, string functionName, object[][] args, Type returnType) {
            var jscp = CodeDomProvider.CreateProvider("JScript");
            var options = new CompilerParameters {
                GenerateExecutable = true,
                GenerateInMemory = false,
            };
            var compilerResults = jscp.CompileAssemblyFromSource(options, js);
            var errors = compilerResults.Errors.Cast<CompilerError>().ToArray();
            // Ignore warning: possibly uninitialized variable
            if (errors.Any(x => x.ErrorNumber != "JS1187")) {
                var errorsStr = errors.Select(x => x.ToString()).ToArray();
                var msg = string.Format("Compile errors:{0}{1}", Environment.NewLine,
                        string.Join(Environment.NewLine, errorsStr));
                throw new InvalidOperationException(msg);
            }

            // Run the compiled JS
            // Difficult to find out exactly how to do this, but this works for now
            var jscriptType = compilerResults.CompiledAssembly.GetType("JScript 0");
            var ret = new object[args.Length];
            for (int i = 0; i < args.Length; i++) {
#pragma warning disable 618
                var vsa = new VsaEngine();
                vsa.RootMoniker = "cil2js://jsrunner";
                vsa.Site = new Site();
                vsa.InitNew();
                GlobalScope gs = new GlobalScope(null, vsa);
                var jscript0 = Activator.CreateInstance(jscriptType, gs);
                var globalCode = jscriptType.GetMethod("Global Code");
                globalCode.Invoke(jscript0, new object[0]);
                var fn = jscriptType.GetMethod(functionName);
                var r = fn.Invoke(jscript0, new object[] { null, vsa }.Concat(args[i]).ToArray());
                if (r.GetType() != returnType) {
                    // Some returns will require casting - e.g. booleans will be returned as integers
                    r = System.Convert.ChangeType(r, returnType);
                }
                ret[i] = r;
#pragma warning restore 618
            }
            return ret;
        }

    }
}
