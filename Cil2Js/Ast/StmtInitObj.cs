using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public class StmtInitObj : Stmt {

        public StmtInitObj(Ctx ctx, Expr destination, TypeReference type)
            : base(ctx) {
            this.Destination = destination;
            this.Type = type;
        }

        public Expr Destination { get; private set; }
        public TypeReference Type { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.InitObj; }
        }

    }
}
