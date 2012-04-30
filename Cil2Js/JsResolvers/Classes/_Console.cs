using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Console {

        [Js(typeof(void), typeof(string))]
        public static Expr WriteLine(ICall call) {
            var ctx = call.Ctx;
            return new ExprJsResolvedMethod(ctx, ctx.Void, null, "console.log", call.Args);
        }

        public static void WriteLine(string format, object arg0) {
            Console.WriteLine(string.Format(format, arg0));
        }

        public static void WriteLine(string format, object arg0, object arg1) {
            Console.WriteLine(string.Format(format, arg0, arg1));
        }

        public static void WriteLine(string format, object arg0, object arg1, object arg2) {
            Console.WriteLine(string.Format(format, arg0, arg1, arg2));
        }

        public static void WriteLine(string format, params object[] args) {
            Console.WriteLine(string.Format(format, args));
        }

    }
}
