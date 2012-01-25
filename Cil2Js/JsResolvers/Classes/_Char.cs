using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    [Js("GetHashCode", typeof(int))]
    class _Char {

        [Js]
        public static Stmt ToString(Ctx ctx) {
            var js = "return String.fromCharCode({0});";
            var stmt = new StmtJsExplicit(ctx, js, ctx.This);
            return stmt;
        }

    }
}
