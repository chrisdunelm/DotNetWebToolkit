using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Cil2Js.Analysis;

namespace Cil2Js.Ast {
    public static class VisitorSameStmt {

        public static bool AreSame(Stmt a, Stmt b) {
            if (a == null && b == null) {
                return true;
            }
            if (a == null || b == null) {
                return false;
            }
            if (a.StmtType != b.StmtType) {
                return false;
            }
            switch (a.StmtType) {
            case Stmt.NodeType.Continuation:
                var aCont = (StmtContinuation)a;
                var bCont = (StmtContinuation)b;
                return aCont.To == bCont.To;
            default:
                return false;
            }
        }

    }
}
