using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class StmtJsExplicit : Stmt {

        public StmtJsExplicit(Ctx ctx, string javaScript, params NamedExpr[] namedExprs)
            : this(ctx, javaScript, (IEnumerable<NamedExpr>)namedExprs) {
        }

        public StmtJsExplicit(Ctx ctx, string javaScript, IEnumerable<NamedExpr> namedExprs)
            : base(ctx) {
            this.JavaScript = javaScript;
            this.NamedExprs = namedExprs;
        }

        public string JavaScript { get; private set; }
        public IEnumerable<NamedExpr> NamedExprs { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return (Stmt.NodeType)JsStmtType.JsExplicit; }
        }

    }
}
