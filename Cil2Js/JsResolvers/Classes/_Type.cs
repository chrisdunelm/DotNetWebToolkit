using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {

    [Js("get_FullName", typeof(string))]
    class _Type {

        [Js]
        public static Expr op_Equality(ICall call) {
            var ctx = call.Ctx;
            var a = call.Args.ElementAt(0);
            var b = call.Args.ElementAt(1);
            var expr = new ExprBinary(ctx, BinaryOp.Equal, ctx.Boolean, a, b);
            return expr;
        }

        [Js]
        public static Expr GetTypeFromHandle(ICall call) {
            return call.Args.First();
        }

        [Js("ToString", typeof(string))]
        public static Stmt ToString(Ctx ctx) {
            var eNamespace = new ExprJsTypeData(ctx, TypeData.Namespace).Named("namespace");
            var eName = new ExprJsTypeData(ctx, TypeData.Name).Named("name");
            var stmt = new StmtJsExplicit(ctx, "return this.namespace+\".\"+this.name;", ctx.ThisNamed, eNamespace, eName);
            return stmt;
        }

    }
}
