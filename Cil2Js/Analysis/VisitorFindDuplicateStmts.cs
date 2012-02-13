using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    class VisitorFindDuplicateStmts : AstRecursiveVisitor {

        public static IEnumerable<Stmt> Find(ICode ast) {
            var v = new VisitorFindDuplicateStmts();
            v.Visit(ast);
            return v.duplicates;
        }

        private HashSet<Stmt> seen = new HashSet<Stmt>();
        private List<Stmt> duplicates = new List<Stmt>();

        private VisitorFindDuplicateStmts() { }

        protected override ICode VisitStmt(Stmt s) {
            if (!this.seen.Add(s)) {
                this.duplicates.Add(s);
                return s;
            } else {
                return base.VisitStmt(s);
            }
        }

    }
}
