using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public static class VisitorSubstituteIrreducable {

        public static ICode V(ICode ast) {
            var ctx = ast.Ctx;
            var blockInfo = FindSuitableBlocks.GetInfo(ast);
            var bestBlock = blockInfo
                .Where(x => !x.Value.containsLeaveProtectedRegion && x.Value.numConts > 0)
                .OrderBy(x => x.Value.numConts)
                .ThenBy(x => x.Value.numICodes)
                .First();
            if (bestBlock.Value.numConts != 1) {
                // Best block must have just one continuation
                return ast;
            }
            var blockAst = (Stmt)bestBlock.Key;
            if (blockAst.StmtType != Stmt.NodeType.Block) {
                // Best block must be StmtBlock
                return ast;
            }
            var stmtBlock = (StmtBlock)blockAst;
            var stmts = stmtBlock.Statements.ToArray();
            if (stmts.Last().StmtType != Stmt.NodeType.Continuation) {
                throw new InvalidOperationException("Last stmt must be continuation");
            }
            var cont = (StmtContinuation)stmts.Last();

            var ifSkipContentPhi = new ExprVarPhi(ctx);
            var ifSkipInitialVar = ctx.Local(ctx.Boolean);
            var ifSkipContentVar = ctx.Local(ctx.Boolean);
            var ifSkipContentPhiExprs = new List<Expr>{ifSkipInitialVar,ifSkipContentVar};
            var ifSkipReset = new StmtAssignment(ctx, ifSkipContentVar, ctx.Literal(false));

            var inIfBlock = new StmtBlock(ctx, stmts.Take(stmts.Length - 1));
            var ifBlock = new StmtIf(ctx, ctx.ExprGen.Not(ifSkipContentPhi), inIfBlock, null);
            var newBlock = new StmtBlock(ctx, ifBlock, ifSkipReset, cont);
            ast = VisitorReplace.V(ast, blockAst, newBlock);

            var allConts = VisitorFindContinuationsRecursive.Get(ast);
            var contsNeedChanging = allConts.Where(x => x != cont && x.To == cont.To).ToArray();
            var contsReplaceInfo = contsNeedChanging
                .Select(x => {
                    var ifVar = ctx.Local(ctx.Boolean);
                    var newCont = new StmtBlock(ctx,
                        new StmtAssignment(ctx, ifVar, ctx.Literal(true)),
                        new StmtContinuation(ctx, newBlock, x.LeaveProtectedRegion));
                    return new { oldCont = x, ifVar, newCont };
                })
                .ToArray();

            foreach (var contReplace in contsReplaceInfo) {
                ifSkipContentPhiExprs.Add(contReplace.ifVar);
                ast = VisitorReplace.V(ast, contReplace.oldCont, contReplace.newCont);
            }
            ifSkipContentPhi.Exprs = ifSkipContentPhiExprs;

            // TODO: Shouldn't be required, but definite-assignment doesn't quite work properly, so is required
            var initalSkipVarAssignment = new StmtAssignment(ctx, ifSkipInitialVar, ctx.Literal(false));
            var newAst = new StmtBlock(ctx, initalSkipVarAssignment, (Stmt)ast);
            
            return newAst;
        }

        class BlockInfo {
            public int numICodes;
            public int numConts;
            public bool containsLeaveProtectedRegion;
            public override string ToString() {
                return string.Format("numICodes: {0}, numConts: {1}", this.numICodes, this.numConts);
            }
        }

        class FindSuitableBlocks : AstRecursiveVisitor {

            public static Dictionary<ICode, BlockInfo> GetInfo(ICode ast) {
                var v = new FindSuitableBlocks();
                v.Visit(ast);
                return v.blockCounts;
            }

            private Dictionary<ICode, BlockInfo> blockCounts = new Dictionary<ICode, BlockInfo>();
            private int numICodes;
            private int numConts;
            private bool containsLeaveProtectedRegion;

            protected override void BlockStart(ICode block) {
                this.numICodes = 0;
                this.numConts = 0;
                this.containsLeaveProtectedRegion = false;
            }

            protected override void BlockEnd(ICode originalBlock, ICode transformedBlock) {
                this.blockCounts.Add(originalBlock, new BlockInfo {
                    numICodes = this.numICodes,
                    numConts = this.numConts,
                    containsLeaveProtectedRegion = this.containsLeaveProtectedRegion,
                });
            }

            public override ICode Visit(ICode c) {
                this.numICodes++;
                return base.Visit(c);
            }

            protected override ICode VisitContinuation(StmtContinuation s) {
                this.numConts++;
                this.containsLeaveProtectedRegion |= s.LeaveProtectedRegion;
                return base.VisitContinuation(s);
            }

        }

    }
}
