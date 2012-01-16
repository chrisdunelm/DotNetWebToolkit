using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Analysis;
using Mono.Cecil;

namespace DotNetWebToolkit.Cil2Js.Ast {
    public abstract class Expr : ICode {

        public enum NodeType {
            Binary,
            Unary,
            Literal,
            VarParameter,
            VarLocal,
            VarExprInstResult,
            VarThis,
            VarPhi,
            Ternary,
            Call,
            FieldAccess,
            NewObj,
            NewArray,
            ArrayAccess,
            ArrayLength,
            Cast,
            MethodReference,
            DefaultValue,
            Assignment,
            Box,
            Unbox,

            Max = 1000
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

        public Expr(Ctx ctx) {
            this.Ctx = ctx;
        }

        public Ctx Ctx { get; private set; }

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

            internal Gen(Ctx ctx) {
                this.ctx = ctx;
            }

            private Ctx ctx;

            public ExprLiteral True() {
                return new ExprLiteral(this.ctx, true, this.ctx.Boolean);
            }

            public ExprLiteral False() {
                return new ExprLiteral(this.ctx, false, this.ctx.Boolean);
            }

            public ExprBinary And(Expr left, Expr right) {
                return new ExprBinary(this.ctx, BinaryOp.And, this.ctx.Boolean, left, right);
            }

            public ExprBinary Or(Expr left, Expr right) {
                return new ExprBinary(this.ctx, BinaryOp.Or, this.ctx.Boolean, left, right);
            }

            public ExprUnary Not(Expr e) {
                return new ExprUnary(this.ctx, UnaryOp.Not, this.ctx.Boolean, e);
            }

            public Expr NotAutoSimplify(Expr e) {
                //return (Expr)VisitorBooleanSimplification.V(Not(e));
                // Special handling for when 'e' already starts with a 'not'
                if (e.ExprType == NodeType.Unary) {
                    var eUn = (ExprUnary)e;
                    if (eUn.Op == UnaryOp.Not) {
                        return eUn.Expr;
                    }
                }
                return Not(e);
            }

            public Expr Add(Expr left, Expr right) {
                return new ExprBinary(this.ctx, BinaryOp.Add, left.Type, left, right);
            }

            public Expr Equal(Expr left, Expr right) {
                return new ExprBinary(this.ctx, BinaryOp.Equal, this.ctx.Boolean, left, right);
            }

            public Expr NotEqual(Expr left, Expr right) {
                return new ExprBinary(this.ctx, BinaryOp.NotEqual, this.ctx.Boolean, left, right);
            }

        }

        public static Gen CreateExprGen(Ctx ctx) {
            return new Gen(ctx);
        }

        object ICloneable.Clone() {
            throw new NotImplementedException();
        }
    }
}
