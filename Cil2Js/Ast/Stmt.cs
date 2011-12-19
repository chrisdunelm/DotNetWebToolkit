using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public abstract class Stmt : ICode {

        public enum NodeType {
            Continuation,
            Cil,

            Empty,
            WrapExpr,
            Block,
            Try,
            Throw,
            Assignment,
            If,
            DoLoop,
            Return,
        }

        public Stmt(Ctx ctx) {
            this.Ctx = ctx;
        }

        public abstract NodeType StmtType { get; }

        public Ctx Ctx { get; private set; }

        CodeType ICode.CodeType {
            get { return CodeType.Statement; }
        }

        class EqualityComparer : IEqualityComparer<Stmt> {
            public bool Equals(Stmt x, Stmt y) {
                return x.DoesEqual(y);
            }

            public int GetHashCode(Stmt obj) {
                // Not possible to calculate a hash code
                return 0;
            }
        }

        public static readonly IEqualityComparer<Stmt> EqComparer = new EqualityComparer();

        object ICloneable.Clone() {
            return this.MemberwiseClone();
        }
    }
}
