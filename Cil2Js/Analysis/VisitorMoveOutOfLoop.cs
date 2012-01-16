using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorMoveOutOfLoop : AstVisitor {

        public static ICode V(ICode ast) {
            var v = new VisitorMoveOutOfLoop();
            return v.Visit(ast);
        }

        private VisitorMoveOutOfLoop() { }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var body = (Stmt)this.Visit(s.Body);
            StmtIf lastIf = null;
            IEnumerable<Stmt> preIf = null;
            if (body.StmtType == Stmt.NodeType.Block) {
                var sBlock = (StmtBlock)body;
                if (sBlock.Statements.Any()) {
                    var sLast = sBlock.Statements.Last();
                    if (sLast.StmtType == Stmt.NodeType.If) {
                        lastIf = (StmtIf)sLast;
                        preIf = sBlock.Statements.Take(sBlock.Statements.Count() - 1).ToArray();
                    }
                }
            } else if (body.StmtType == Stmt.NodeType.If) {
                lastIf = (StmtIf)body;
                preIf = Enumerable.Empty<Stmt>();
            }
            if (lastIf != null) {
                Stmt afterLoop = null;
                StmtIf newIf = null;
                // See if final 'if' condition is same as the 'do' condition.
                // TODO: This may lead to a non-terminating situation...
                if (lastIf.Condition.DoesEqual(s.While)) {
                    afterLoop = lastIf.Else;
                    newIf = new StmtIf(s.Ctx, lastIf.Condition, lastIf.Then, null);
                } else if (lastIf.Condition.DoesEqualNot(s.While)) {
                    afterLoop = lastIf.Then;
                    newIf = new StmtIf(s.Ctx, lastIf.Condition, null, lastIf.Else);
                }
                if (afterLoop != null) {
                    var loopBody = new StmtBlock(s.Ctx, preIf.Concat(newIf));
                    var loop = new StmtDoLoop(s.Ctx, loopBody, s.While);
                    var ret = new StmtBlock(s.Ctx, loop, afterLoop);
                    return ret;
                }
            }
            if (body != s.Body) {
                return new StmtDoLoop(s.Ctx, body, s.While);
            } else {
                return s;
            }
        }

    }
}
