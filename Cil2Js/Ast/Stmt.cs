using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cil2Js.Ast {
    public abstract class Stmt : ICode {

        public enum NodeType {
            Continuation,
            Cil,

            Call,
            Block,
            Try,
            Throw,
            Assignment,
            If,
            DoLoop,
            Return,
        }

        public abstract NodeType StmtType { get; }

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
