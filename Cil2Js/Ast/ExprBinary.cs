using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {

    public enum BinaryOp {
        Add,
        Sub,
        Mul,
        Div,

        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,

        NotEqual,
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,

        And,
        Or,
    }

    public class ExprBinary:Expr {

        public ExprBinary(BinaryOp op, TypeReference type, Expr left, Expr right) {
            this.Op = op;
            this.type = type;
            this.Left = left;
            this.Right = right;
        }

        private TypeReference type;

        public BinaryOp Op { get; private set; }
        public Expr Left { get; private set; }
        public Expr Right { get; private set; }

        public override NodeType ExprType {
            get { return NodeType.Binary; }
        }

        public override TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return string.Format("({0} {1} {2})", this.Left, this.Op, this.Right);
        }

    }
}
