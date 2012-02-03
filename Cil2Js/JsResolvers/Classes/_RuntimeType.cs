using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Output;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    
    class _RuntimeType {

        [Js("ToString", typeof(string))]
        public static Stmt ToString(Ctx ctx) {
            var eNamespace = new ExprJsTypeData(ctx, TypeData.Namespace).Named("namespace");
            var eName = new ExprJsTypeData(ctx, TypeData.Name).Named("name");
            var stmt = new StmtJsExplicit(ctx, "return this.namespace+\".\"+this.name;", ctx.ThisNamed, eNamespace, eName);
            return stmt;
        }

        [Js]
        public static Stmt get_FullName(Ctx ctx) {
            var eNamespace = new ExprJsTypeData(ctx, TypeData.Namespace).Named("namespace");
            var eName = new ExprJsTypeData(ctx, TypeData.Name).Named("name");
            var stmt = new StmtJsExplicit(ctx, "return this.namespace+\".\"+this.name;", ctx.ThisNamed, eNamespace, eName);
            return stmt;
        }

    }
}
