using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class VisitorJsReturnThis : JsAstVisitor {

        public static ICode V(ICode ast, Expr eReturn) {
            var v = new VisitorJsReturnThis(eReturn);
            return v.Visit(ast);
        }

        private VisitorJsReturnThis(Expr eReturn) {
            this.eReturn = eReturn;
        }

        private Expr eReturn;

        protected override ICode VisitReturn(StmtReturn s) {
            return new StmtReturn(s.Ctx, this.eReturn);
        }

    }
}
