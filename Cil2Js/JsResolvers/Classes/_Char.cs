using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    class _Char {

        [JsRedirect(typeof(char))]
        public override string ToString() {
            throw new JsImplException();
        }
        [Js]
        public static Stmt ToString(Ctx ctx) {
            var js = "return String.fromCharCode(this);";
            var stmt = new StmtJsExplicit(ctx, js, ctx.ThisNamed);
            return stmt;
        }

        [JsRedirect(typeof(char))]
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        [Js]
        public static Stmt GetHashCode(Ctx ctx) {
            return new StmtJsExplicit(ctx, "return this | this << 16;", ctx.ThisNamed);
        }

    }
}
