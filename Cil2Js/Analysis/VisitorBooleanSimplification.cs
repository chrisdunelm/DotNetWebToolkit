using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Ast;
using Mono.Cecil;

namespace Cil2Js.Analysis {
    public class VisitorBooleanSimplification : AstRecursiveVisitor {

        // Implements the Quine-McCluskey algorithm; see http://en.wikipedia.org/wiki/Quine%E2%80%93McCluskey_algorithm

        public static ICode V(ICode ast) {
            HashSet<Expr> dummy;
            return V(ast, new HashSet<Expr>(), out dummy);
        }

        public static ICode V(ICode ast, HashSet<Expr> previouslySimplified, out HashSet<Expr> thisSimplified) {
            var v = new VisitorBooleanSimplification(previouslySimplified);
            var ret = v.Visit(ast);
            thisSimplified = v.thisSimplified;
            return ret;
        }

        private VisitorBooleanSimplification(HashSet<Expr> previouslySimplified) {
            this.previouslySimplified = previouslySimplified;
        }

        private HashSet<Expr> previouslySimplified;
        private HashSet<Expr> thisSimplified = new HashSet<Expr>();

        // AB is A && B
        // A + B is A || B

        protected override ICode VisitUnary(ExprUnary e) {
            if (e.Op == UnaryOp.Not) {
                return this.Algo(e);
            }
            return base.VisitUnary(e);
        }

        protected override ICode VisitBinary(ExprBinary e) {
            if (e.Op == BinaryOp.And || e.Op == BinaryOp.Or) {
                return this.Algo(e);
            }
            return base.VisitBinary(e);
        }

        class VisitorFindVariables : AstVisitor {

            public static Tuple<Expr[], int> V(Expr e) {
                var v = new VisitorFindVariables();
                v.Visit(e);
                return Tuple.Create(v.variables.ToArray(), v.varCount);
            }

            private HashSet<Expr> variables = new HashSet<Expr>();
            private int varCount = 0;

            protected override ICode VisitExpr(Expr e) {
                switch (e.ExprType) {
                case Expr.NodeType.Unary:
                    var eUn = (ExprUnary)e;
                    if (eUn.Op == UnaryOp.Not) {
                        this.Visit(eUn.Expr);
                        return e;
                    }
                    break;
                case Expr.NodeType.Binary:
                    var eBin = (ExprBinary)e;
                    if (eBin.Op == BinaryOp.And || eBin.Op == BinaryOp.Or) {
                        this.Visit(eBin.Left);
                        this.Visit(eBin.Right);
                        return e;
                    }
                    break;
                case Expr.NodeType.Literal:
                    return e;
                }
                this.variables.Add(e);
                this.varCount++;
                return e;
            }

        }

        class VisitorGetTruthTableOnCover : AstVisitor {

            public static IEnumerable<int> V(Expr e, Expr[] variables) {
                var v = new VisitorGetTruthTableOnCover();
                var iterations = 1 << variables.Length;
                for (int i = 0; i < iterations; i++) {
                    for (int j = 0; j < variables.Length; j++) {
                        v.values[variables[j]] = (i & (1 << j)) != 0;
                    }
                    v.stack.Clear();
                    v.Visit(e);
                    var value = v.stack.Pop();
                    if (value) {
                        yield return i;
                    }
                }
            }

            private Dictionary<Expr, bool> values = new Dictionary<Expr, bool>();
            private Stack<bool> stack = new Stack<bool>();

            protected override ICode VisitExpr(Expr e) {
                bool value;
                if (this.values.TryGetValue(e, out value)) {
                    this.stack.Push(value);
                    return e;
                } else {
                    return base.VisitExpr(e);
                }
            }

            protected override ICode VisitUnary(ExprUnary e) {
                if (e.Op != UnaryOp.Not) {
                    throw new InvalidOperationException(e.Op.ToString());
                }
                this.Visit(e.Expr);
                var value = this.stack.Pop();
                this.stack.Push(!value);
                return e;
            }

            protected override ICode VisitBinary(ExprBinary e) {
                this.Visit(e.Left);
                this.Visit(e.Right);
                var r = this.stack.Pop();
                var l = this.stack.Pop();
                bool value;
                switch (e.Op) {
                case BinaryOp.And: value = l && r; break;
                case BinaryOp.Or: value = l || r; break;
                default: throw new InvalidOperationException(e.Op.ToString());
                }
                this.stack.Push(value);
                return e;
            }

            protected override ICode VisitLiteral(ExprLiteral e) {
                this.stack.Push(e.IsLiteralBoolean(true));
                return e;
            }

        }

        private int BitsSet(int i) {
            int r = 0;
            while (i != 0) {
                if ((i & 1) != 0) {
                    r++;
                }
                i >>= 1;
            }
            return r;
        }

        private IEnumerable<int> OnBits(int i) {
            int c = 0;
            while (i != 0) {
                if ((i & 1) != 0) {
                    yield return c;
                }
                c++;
                i >>= 1;
            }
        }

        struct Inputs {
            public Inputs(int on, int dc, int[] covers) {
                this.on = on;
                this.dc = dc;
                this.covers = covers;
            }
            public int on; // Bits must be zero when dc bit is 1
            public int dc;
            public int[] covers;
            public override bool Equals(object obj) {
                if (obj is Inputs) {
                    var other = (Inputs)obj;
                    return this.on == other.on && this.dc == other.dc;
                }
                return false;
            }
            public override int GetHashCode() {
                return this.on ^ (this.dc << 8);
            }
            public override string ToString() {
                return string.Format("{{ on:{0:x} dc:{1:x} }}", this.on, this.dc);
            }
        }

        private Expr Algo(Expr e) {
            if (this.previouslySimplified.Contains(e)) {
                this.thisSimplified.Add(e);
                return e;
            }
            var variablesInfo = VisitorFindVariables.V(e);
            var variables = variablesInfo.Item1;
            var varCount = variablesInfo.Item2;
            var onCover = VisitorGetTruthTableOnCover.V(e, variables).Select(x => new Inputs(x, 0, new[] { x })).ToArray();
            var orgOnCover = onCover;

            for (; ; ) {
                var byBitsSet = onCover.GroupBy(x => this.BitsSet(x.on)).OrderBy(x => x.Key).ToArray();
                var onCover2 = new List<Inputs>();
                var toRemove = new List<Inputs>();
                for (int i = 1; i < byBitsSet.Length; i++) {
                    var a = byBitsSet[i - 1];
                    var b = byBitsSet[i];
                    if (a.Key == b.Key - 1) {
                        var aGroup = a.ToArray();
                        var bGroup = b.ToArray();
                        for (int j = 0; j < aGroup.Length; j++) {
                            for (int k = 0; k < bGroup.Length; k++) {
                                var aj = aGroup[j];
                                var bk = bGroup[k];
                                if (aj.dc == bk.dc) {
                                    var bitsDiff = aj.on ^ bk.on;
                                    var numBitsDiff = this.BitsSet(bitsDiff);
                                    if (numBitsDiff == 1) {
                                        // Can combine
                                        var input = new Inputs(aj.on, aj.dc | bitsDiff, aj.covers.Union(bk.covers).ToArray());
                                        onCover2.Add(input);
                                        toRemove.Add(aj);
                                        toRemove.Add(bk);
                                    }
                                }
                            }
                        }
                    }
                }
                if (!onCover2.Any()) {
                    break;
                }
                onCover = onCover.Except(toRemove).Concat(onCover2).Distinct().ToArray();
            }

            // TODO: Remove unneeded minterms properly
            List<Inputs> toUse = new  List<Inputs>();
            var requireCovers = orgOnCover.Select(x=>x.on).ToArray();
            do {
                var toAdd = onCover.First(x => !toUse.Contains(x));
                toUse.Add(toAdd);
            } while (requireCovers.Except(toUse.SelectMany(x => x.covers)).Any());

            Expr retExpr = null;
            int retVarCount = 0;
            foreach (var input in toUse) {
                var andVars = variables.Select((x, i) => {
                    var mask = 1 << i;
                    if ((input.dc & mask) != 0) {
                        return null;
                    }
                    if ((input.on & mask) != 0) {
                        return x;
                    }
                    return e.Ctx.ExprGen.Not(x);
                })
                 .Where(x => x != null)
                 .ToArray();
                var andExpr = andVars.Any() ? andVars.Aggregate((a, b) => e.Ctx.ExprGen.And(a, b)) : e.Ctx.ExprGen.True();
                retExpr = retExpr == null ? andExpr : e.Ctx.ExprGen.Or(retExpr, andExpr);
                retVarCount += andVars.Length;
            }

            if (retVarCount < varCount) {
                retExpr = retExpr ?? e.Ctx.ExprGen.False();
            } else {
                retExpr = e;
            }
            this.thisSimplified.Add(retExpr);
            return retExpr;
        }

    }
}
