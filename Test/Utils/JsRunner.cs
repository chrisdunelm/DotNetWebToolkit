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

        public static object Run(string js, string functionName, object[] args) {
            return Run(js, functionName, new[] { args })[0];
        }

        public static object[] Run(string js, string functionName, object[][] args) {
            var info = new AppDomainSetup {
                ApplicationBase = Path.GetDirectoryName(typeof(JsRunner).Assembly.Location)
            };
            var jsDomain = AppDomain.CreateDomain("JsRunner", null, info);
            var jsRunner = (JsRunner)jsDomain.CreateInstanceAndUnwrap(typeof(JsRunner).Assembly.FullName, typeof(JsRunner).FullName);
            var ret = jsRunner.CompileAndExecute(js, functionName, args);
            AppDomain.Unload(jsDomain);
            return ret;
        }

        private object[] CompileAndExecute(string js, string functionName, object[][] args) {
            var jscp = new JScriptCodeProvider();
            var options = new CompilerParameters {
                GenerateExecutable = true,
                GenerateInMemory = true,
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

            var functionQuery =
                from type in compilerResults.CompiledAssembly.GetTypes()
                from method in type.GetMethods()
                where method.Name == functionName
                select method;
            var function = functionQuery.First();
            var ret = new object[args.Length];
            for (int i = 0; i < args.Length; i++) {
                ret[i] = function.Invoke(null, new object[] { null, null }.Concat(args[i]).ToArray());
            }
            return ret;
        }

    }
}
