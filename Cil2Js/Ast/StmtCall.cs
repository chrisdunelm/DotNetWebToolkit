using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public class StmtCall : Stmt, ICall {

        public StmtCall(MethodReference calling, IEnumerable<Expr> args) {
            this.Calling = calling;
            this.Args = args;
        }

        public MethodReference Calling { get; private set; }
        public IEnumerable<Expr> Args { get; private set; }

        public override Stmt.NodeType StmtType {
            get { return NodeType.Call; }
        }

        public TypeReference Type {
            get { return this.Calling.Module.TypeSystem.Void; }
        }

    }
}
