using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    [Js("GetHashCode", typeof(int))]
    [Js("ToString", typeof(string))]
    class _Boolean {

        [Js("Equals", typeof(bool), typeof(object))]
        public static Stmt Equals(Ctx ctx) {
            var other = ctx.MethodParameter(0).Named("other");
            var p0 = new ExprVarParameter(ctx, ctx.MDef.Parameters[0]);
            var stmt = new StmtJsExplicit(ctx, "return this._===other._&&this.v===other.v;", ctx.ThisNamed, other);
            return stmt;
        }

    }
}
