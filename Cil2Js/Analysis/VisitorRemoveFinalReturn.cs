using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;

namespace Cil2Js.Analysis {
    public class VisitorRemoveFinalReturn : AstVisitor {

        public static ICode V(ICode ast) {
            var s = (Stmt)ast;
            if (s.StmtType == Stmt.NodeType.Return) {
                var sRet = (StmtReturn)s;
                if (sRet.Expr == null) {
                    return new StmtEmpty(ast.Ctx);
                }
            }
            if (s.StmtType == Stmt.NodeType.Block) {
                var sBlock = (StmtBlock)s;
                var last = sBlock.Statements.LastOrDefault();
                if (last != null && last.StmtType == Stmt.NodeType.Return) {
                    var lastRet = (StmtReturn)last;
                    if (lastRet.Expr == null) {
                        return new StmtBlock(ast.Ctx, sBlock.Statements.TakeWhile(x => x != last));
                    }
                }
            }
            return ast;
        }


    }
}
