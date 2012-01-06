using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {
    public class StmtBlock : Stmt {

        public StmtBlock(Ctx ctx, params Stmt[] statements)
            : this(ctx, (IEnumerable<Stmt>)statements) {
        }

        public StmtBlock(Ctx ctx, IEnumerable<Stmt> statements)
            : base(ctx) {
            // Expand any nested blocks. Evaluate to array.
            this.Statements = statements.SelectMany(x => {
                if (x == null) {
                    return Enumerable.Empty<Stmt>();
                }
                if (x.StmtType == NodeType.Block) {
                    return ((StmtBlock)x).Statements;
                }
                return new[] { x };
            }).Where(x => x != null).ToArray();
        }

        public IEnumerable<Stmt> Statements { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Block; }
        }

    }
}
