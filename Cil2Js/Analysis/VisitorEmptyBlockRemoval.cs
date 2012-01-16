using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorEmptyBlockRemoval : AstRecursiveVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorEmptyBlockRemoval();
            return v.Visit(ast);
        }

        protected override ICode VisitBlock(StmtBlock s) {
            var count = s.Statements.Count();
            if (count == 0) {
                return null;
            }
            if (count == 1) {
                return this.Visit(s.Statements.First());
            }
            return base.VisitBlock(s);
        }

    }
}
