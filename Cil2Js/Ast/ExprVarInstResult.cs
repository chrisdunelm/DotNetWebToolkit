using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Cil2Js.Ast {
    public class ExprVarInstResult : ExprVar {

        public ExprVarInstResult(Instruction inst, TypeReference type) {
            this.Inst = inst;
            this.type = type;
        }

        private TypeReference type;

        public Instruction Inst { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.VarExprInstResult; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return string.Format("InstResult<IL_{0:x4}>", this.Inst.Offset);
        }

    }
}
