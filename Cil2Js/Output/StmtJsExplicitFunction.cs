using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class StmtJsExplicitFunction : Stmt {

        public StmtJsExplicitFunction(Ctx ctx, string javaScript, params Expr[] exprs)
            : this(ctx, javaScript, (IEnumerable<Expr>)exprs) {
        }

        public StmtJsExplicitFunction(Ctx ctx, string javaScript, IEnumerable<Expr> exprs)
            : base(ctx) {
            this.JavaScript = javaScript;
            this.Exprs = exprs;
        }

        public string JavaScript { get; private set; }
        public IEnumerable<Expr> Exprs { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return (Stmt.NodeType)JsStmtType.JsExplicitFunction; }
        }

    }
}
