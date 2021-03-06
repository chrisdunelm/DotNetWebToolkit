﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {

    public enum UnaryOp {
        Not,
        BitwiseNot,
        Negate,
    }

    public class ExprUnary : Expr {

        public ExprUnary(Ctx ctx, UnaryOp op, TypeReference type, Expr expr)
            : base(ctx) {
            this.Op = op;
            this.type = type;
            this.Expr = expr;
        }

        private TypeReference type;

        public UnaryOp Op { get; private set; }
        public Expr Expr { get; private set; }

        public override Expr.NodeType ExprType {
            get { return NodeType.Unary; }
        }

        public override Mono.Cecil.TypeReference Type {
            get { return this.type; }
        }

        public override string ToString() {
            return string.Format("({0} {1})", this.Op, this.Expr);
        }

    }
}
