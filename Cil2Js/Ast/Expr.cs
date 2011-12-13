using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Cil2Js.Ast {
    public abstract class Expr : ICode {

        public enum NodeType {
            Binary,
            Unary,
            Literal,
            VarParameter,
            VarLocal,
            VarExprInstResult,
            This,
            VarPhi,
            Ternary,
            Call,
            LoadField,
            NewObj,
        }

        [Flags]
        public enum Special {
            None = 0,
            /// <summary>
            /// When evaluating an expression may cause side-effects, therefore the evaluation must be
            /// done even if the expression result is never used, unless it can be proven that there are no side-effects.
            /// e.g.: method calls.
            /// </summary>
            PossibleSideEffects = 1,
        }

        public abstract NodeType ExprType { get; }
        public abstract TypeReference Type { get; }
        public virtual Special Specials {
            get { return Special.None; }
        }

        CodeType ICode.CodeType {
            get { return CodeType.Expression; }
        }

        public override string ToString() {
            return "???";
        }

        class EqualityComparer : IEqualityComparer<Expr> {

            public EqualityComparer(bool exact) {
                this.exact = exact;
            }

            private bool exact;

            public bool Equals(Expr x, Expr y) {
                return this.exact ? x.DoesEqualExact(y) : x.DoesEqual(y);
            }

            public int GetHashCode(Expr obj) {
                // Not possible to calculate a hash code
                return 0;
            }
        }

        public static readonly IEqualityComparer<Expr> EqComparer = new EqualityComparer(false);
        public static readonly IEqualityComparer<Expr> EqComparerExact = new EqualityComparer(true);

        public class Gen {

            internal Gen(TypeSystem ts) {
                this.ts = ts;
            }

            private TypeSystem ts;

            public ExprBinary And(Expr left, Expr right) {
                return new ExprBinary(BinaryOp.And, this.ts.Boolean, left, right);
            }

            public ExprBinary Or(Expr left, Expr right) {
                return new ExprBinary(BinaryOp.Or, this.ts.Boolean, left, right);
            }

            public ExprUnary Not(Expr e) {
                return new ExprUnary(UnaryOp.Not, this.ts.Boolean, e);
            }

            public Expr NotAutoSimplify(Expr e) {
                // Special handling for when 'e' already starts with a 'not'
                if (e.ExprType == NodeType.Unary) {
                    var eUn = (ExprUnary)e;
                    if (eUn.Op == UnaryOp.Not) {
                        return eUn.Expr;
                    }
                }
                return Not(e);
            }

            public Expr Equal(Expr left, Expr right) {
                return new ExprBinary(BinaryOp.Equal, this.ts.Boolean, left, right);
            }

            public Expr NotEqual(Expr left, Expr right) {
                return new ExprBinary(BinaryOp.NotEqual, this.ts.Boolean, left, right);
            }

        }

        public static Gen ExprGen(TypeSystem ts) {
            return new Gen(ts);
        }

        object ICloneable.Clone() {
            throw new NotImplementedException();
        }
    }
}
