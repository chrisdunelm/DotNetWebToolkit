using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Ast;
using DotNetWebToolkit.Cil2Js.Utils;

namespace DotNetWebToolkit.Cil2Js.Analysis {
    public class VisitorBooleanSimplification : AstRecursiveVisitor {

        // Implements the Quine-McCluskey algorithm; see http://en.wikipedia.org/wiki/Quine%E2%80%93McCluskey_algorithm
        // with simplification of known terms

        public static ICode V(ICode ast) {
            var v = new VisitorBooleanSimplification();
            return v.Visit(ast);
        }

        private VisitorBooleanSimplification() {
            this.knownTrue = new Stack<List<Expr>>();
            this.knownTrue.Push(new List<Expr>());
            this.assignmentsTo = new List<Expr>();
        }

        private Stack<List<Expr>> knownTrue;
        private List<Expr> assignmentsTo;

        protected override void BlockStart(ICode block) {
            base.BlockStart(block);
            this.knownTrue.Clear();
            this.knownTrue.Push(new List<Expr>());
            this.assignmentsTo.Clear();
        }

        private void AddKnownTrue(Expr e) {
            this.knownTrue.Peek().Add(e);
        }

        private void AddKnownTrue(IEnumerable<Expr> es) {
            this.knownTrue.Peek().AddRange(es);
        }

        private bool HasBlockExit(Stmt s) {
            if (s == null) {
                return false;
            }
            if (s.StmtType == Stmt.NodeType.Throw || s.StmtType == Stmt.NodeType.Return || s.StmtType == Stmt.NodeType.Continuation) {
                return true;
            }
            if (s.StmtType == Stmt.NodeType.Block) {
                return ((StmtBlock)s).Statements.Any(x => x.StmtType == Stmt.NodeType.Throw || x.StmtType == Stmt.NodeType.Return || x.StmtType == Stmt.NodeType.Continuation);
            }
            return false;
        }

        protected override ICode VisitIf(StmtIf s) {
            var ctx = s.Ctx;
            var eg = ctx.ExprGen;
            var condition = (Expr)this.Visit(s.Condition);
            var notCondition = eg.Not(condition);

            var assignmentsTo = this.assignmentsTo;
            this.assignmentsTo = new List<Expr>();

            this.knownTrue.Push(new List<Expr>());
            this.AddKnownTrue(condition);
            var then = (Stmt)this.Visit(s.Then);
            var thenTrue = this.knownTrue.Pop();
            var thenAssignmentsTo = this.assignmentsTo;

            this.assignmentsTo = new List<Expr>();
            this.knownTrue.Push(new List<Expr>());
            this.AddKnownTrue(notCondition);
            var @else = (Stmt)this.Visit(s.Else);
            var elseTrue = this.knownTrue.Pop();
            var elseAssignmentsTo = this.assignmentsTo;

            this.assignmentsTo = assignmentsTo.Concat(thenAssignmentsTo).Concat(elseAssignmentsTo).ToList();

            if (this.HasBlockExit(s.Then)) {
                this.AddKnownTrue(elseTrue);
            } else {
                var known = thenTrue.Skip(1).Select(x => eg.Or(notCondition, x)).ToArray();
                this.AddKnownTrue(known);
            }
            if (this.HasBlockExit(s.Else)) {
                this.AddKnownTrue(thenTrue);
            } else {
                var known = elseTrue.Skip(1).Select(x => eg.Or(condition, x)).ToArray();
                this.AddKnownTrue(known);
            }

            var thenAssignmentTruths = thenAssignmentsTo.Select(x => eg.Or(condition, eg.Not(x))).ToArray();
            this.AddKnownTrue(thenAssignmentTruths);
            var elseAssignmentTruths = elseAssignmentsTo.Select(x => eg.Or(notCondition, eg.Not(x))).ToArray();
            this.AddKnownTrue(elseAssignmentTruths);

            if (condition != s.Condition || then != s.Then || @else != s.Else) {
                return new StmtIf(ctx, condition, then, @else);
            } else {
                return s;
            }
        }

        protected override ICode VisitDoLoop(StmtDoLoop s) {
            var ctx = s.Ctx;
            var body = (Stmt)this.Visit(s.Body);
            var @while = (Expr)this.Visit(s.While); // This order matters - body must be visited before while
            GetVarsVisitor.GetAll(@while);
            this.AddKnownTrue(ctx.ExprGen.Not(@while));
            if (body != s.Body || @while != s.While) {
                return new StmtDoLoop(ctx, body, @while);
            } else {
                return s;
            }
        }

        protected override ICode VisitTry(StmtTry s) {
            // 'try' part ends unpredicably, so cannot use any information
            // 'catch' part is called unpredictably, so cannot use any information
            // 'finally' part is always run, so can use all information
            this.knownTrue.Push(new List<Expr>());
            var @try = (Stmt)this.Visit(s.Try);
            this.knownTrue.Pop();
            var catches = this.HandleList(s.Catches, x => {
                this.knownTrue.Push(new List<Expr>());
                var @catch = (Stmt)this.Visit(x.Stmt);
                this.knownTrue.Pop();
                if (@catch != x.Stmt) {
                    return new StmtTry.Catch(@catch, x.ExceptionVar);
                } else {
                    return x;
                }
            });
            var @finally = (Stmt)this.Visit(s.Finally);
            if (@try != s.Try || catches != null || @finally != s.Finally) {
                return new StmtTry(s.Ctx, @try, catches ?? s.Catches, @finally);
            } else {
                return s;
            }
        }

        protected override ICode VisitAssignment(StmtAssignment s) {
            if (s.Target.Type.IsBoolean()) {
                this.assignmentsTo.Add(s.Target);
            }
            return base.VisitAssignment(s);
        }

        protected override ICode VisitAssignment(ExprAssignment e) {
            if (e.Target.Type.IsBoolean()) {
                this.assignmentsTo.Add(e.Target);
            }
            return base.VisitAssignment(e);
        }

        protected override ICode VisitExpr(Expr e) {
            bool runSimplification;
            switch (e.ExprType) {
            case Expr.NodeType.Unary:
                var eUnary = (ExprUnary)e;
                runSimplification = eUnary.Op == UnaryOp.Not;
                break;
            case Expr.NodeType.Binary:
                var eBinary = (ExprBinary)e;
                runSimplification = eBinary.Op == BinaryOp.And || eBinary.Op == BinaryOp.Or;
                break;
            default:
                runSimplification = e.IsVar();
                break;
            }
            if (!runSimplification) {
                return base.VisitExpr(e);
            }

            var knownTrue = this.knownTrue.SelectMany(x => x).ToArray();
            var simplified = QuineMcCluskey(e, knownTrue);
            return simplified;
        }

        public abstract class BooleanVisitor : AstVisitor {

            protected virtual void BooleanVisitNot(ExprUnary e) {
                this.Visit(e.Expr);
            }

            protected virtual void BooleanVisitAnd(ExprBinary e) {
                this.Visit(e.Left);
                this.Visit(e.Right);
            }

            protected virtual void BooleanVisitOr(ExprBinary e) {
                this.Visit(e.Left);
                this.Visit(e.Right);
            }

            protected virtual void BooleanVisitLiteral(ExprLiteral e) {
            }

            protected virtual void BooleanVisitVar(Expr e) {
            }

            protected override ICode VisitExpr(Expr e) {
                var isVar = true;
                switch (e.ExprType) {
                case Expr.NodeType.Unary:
                    var eUnary = (ExprUnary)e;
                    if (eUnary.Op == UnaryOp.Not) {
                        this.BooleanVisitNot(eUnary);
                        isVar = false;
                    }
                    break;
                case Expr.NodeType.Binary:
                    var eBinary = (ExprBinary)e;
                    switch (eBinary.Op) {
                    case BinaryOp.And:
                        this.BooleanVisitAnd(eBinary);
                        isVar = false;
                        break;
                    case BinaryOp.Or:
                        this.BooleanVisitOr(eBinary);
                        isVar = false;
                        break;
                    }
                    break;
                case Expr.NodeType.Literal:
                    var eLiteral = (ExprLiteral)e;
                    this.BooleanVisitLiteral(eLiteral);
                    isVar = false;
                    break;
                }
                if (isVar) {
                    this.BooleanVisitVar(e);
                }
                return e;
            }

        }

        public class GetVarsVisitor : BooleanVisitor {

            public static IEnumerable<Expr> GetAll(Expr e) {
                var v = new GetVarsVisitor();
                v.Visit(e);
                return v.variables;
            }

            private List<Expr> variables = new List<Expr>();

            protected override void BooleanVisitVar(Expr e) {
                this.variables.Add(e);
            }

        }

        public class EvalVisitor : BooleanVisitor {

            public static bool Eval(Bits bits, Dictionary<Expr, int> exprMap, Expr e) {
                var v = new EvalVisitor {
                    bits = bits,
                    exprMap = exprMap,
                };
                v.Visit(e);
                return v.stack.Pop();
            }

            private Bits bits;
            private Dictionary<Expr, int> exprMap;
            private Stack<bool> stack = new Stack<bool>();

            protected override void BooleanVisitNot(ExprUnary e) {
                base.BooleanVisitNot(e);
                var value = this.stack.Pop();
                this.stack.Push(!value);
            }

            protected override void BooleanVisitAnd(ExprBinary e) {
                base.BooleanVisitAnd(e);
                var right = this.stack.Pop();
                var left = this.stack.Pop();
                this.stack.Push(left && right);
            }

            protected override void BooleanVisitOr(ExprBinary e) {
                base.BooleanVisitOr(e);
                var right = this.stack.Pop();
                var left = this.stack.Pop();
                this.stack.Push(left || right);
            }

            protected override void BooleanVisitLiteral(ExprLiteral e) {
                if (e.IsLiteralBoolean(true)) {
                    this.stack.Push(true);
                } else if (e.IsLiteralBoolean(false)) {
                    this.stack.Push(false);
                } else {
                    throw new NotSupportedException("This should never occur - non-boolean literal in boolean expression");
                }
            }

            protected override void BooleanVisitVar(Expr e) {
                var value = this.bits[this.exprMap[e]];
                this.stack.Push(value);
            }

        }

        public struct Bits {

            public Bits(int value)
                : this() {
                this.Value = value;
            }

            public int Value { get; private set; }

            public bool this[int bit] {
                get {
                    return (this.Value & (1 << bit)) != 0;
                }
            }

            public int BitCount {
                get {
                    int v = this.Value;
                    int ret = 0;
                    while (v > 0) {
                        if ((v & 1) != 0) {
                            ret++;
                        }
                        v >>= 1;
                    }
                    return ret;
                }
            }

            public override string ToString() {
                return string.Format("0x{0:x2} bc={1}", this.Value, this.BitCount);
            }

        }

        class Implicant {

            class EqualityComparer : IEqualityComparer<Implicant> {
                public bool Equals(Implicant x, Implicant y) {
                    return x.Bits.Value == y.Bits.Value &&
                        x.DoesntMatter == y.DoesntMatter &&
                        Enumerable.SequenceEqual(x.Covers, y.Covers);
                }

                public int GetHashCode(Implicant obj) {
                    var hc = obj.Bits.Value.GetHashCode() ^ obj.DoesntMatter.GetHashCode() << 6;
                    foreach (var i in obj.Covers) {
                        hc ^= i.GetHashCode() << 10;
                    }
                    return hc;
                }
            }

            public static readonly IEqualityComparer<Implicant> EqComparerInstance = new EqualityComparer();

            public Implicant(Bits bits, int doesntMatter, int[] covers) {
                this.Bits = bits;
                this.DoesntMatter = doesntMatter;
                this.Covers = covers;
                this.Combined = false;
            }

            public Bits Bits { get; private set; }
            public int DoesntMatter { get; private set; }
            public int[] Covers { get; private set; }
            public bool Combined { get; set; }

            public override string ToString() {
                return string.Format("{{ Bits:0x{0:x2} dm:0x{1:x2} (bc={2}) covers:[{3}] }}",
                    this.Bits.Value, this.DoesntMatter, this.Bits.BitCount,
                    string.Join(", ", this.Covers.Select(x => x.ToString())));
            }

        }

        private static IEnumerable<Bits> EnumBits(Expr[] exprs) {
            var n = 1 << exprs.Length;
            for (int i = 0; i < n; i++) {
                yield return new Bits(i);
            }
        }

        public static Expr QuineMcCluskey(Expr e, IEnumerable<Expr> knownTrue) {
            HashSet<Expr> variables = new HashSet<Expr>();
            Action<Expr> findVars = null;
            findVars = x => {
                bool isVar = true;
                switch (x.ExprType) {
                case Expr.NodeType.Unary:
                    var eUnary = (ExprUnary)x;
                    if (eUnary.Op == UnaryOp.Not) {
                        findVars(eUnary.Expr);
                        isVar = false;
                    }
                    break;
                case Expr.NodeType.Binary:
                    var eBinary = (ExprBinary)x;
                    if (eBinary.Op == BinaryOp.And || eBinary.Op == BinaryOp.Or) {
                        findVars(eBinary.Left);
                        findVars(eBinary.Right);
                        isVar = false;
                    }
                    break;
                case Expr.NodeType.Literal:
                    isVar = false;
                    break;
                }
                if (isVar) {
                    variables.Add(x);
                }
            };
            findVars(e);

            var useKnownTrue = knownTrue.Where(x => GetVarsVisitor.GetAll(x).All(y => variables.Contains(y))).ToArray();
            var exprs = variables.ToArray();
            var exprMap = exprs.Select((x, i) => new { x, i }).ToDictionary(x => x.x, x => x.i);

            var truthTable = EnumBits(exprs)
                .Select(bits => {
                    foreach (var known in useKnownTrue) {
                        var knownResult = EvalVisitor.Eval(bits, exprMap, known);
                        if (!knownResult) {
                            return null;
                        }
                    }
                    return (bool?)EvalVisitor.Eval(bits, exprMap, e);
                })
                .ToArray();

            var nextStageImplicants = new HashSet<Implicant>(truthTable
                .Select((x, i) => new { bits = new Bits(i), x })
                .Where(x => !x.x.HasValue || x.x.Value)
                .Select(x => new Implicant(x.bits, 0, new[] { x.bits.Value })), Implicant.EqComparerInstance);

            var primeImplicants = new List<Implicant>();

            while (nextStageImplicants.Any()) {
                var minTerms = nextStageImplicants
                    .GroupBy(x => x.Bits.BitCount)
                    .OrderBy(x => x.Key)
                    .ToArray();
                nextStageImplicants.Clear();
                Implicant[] sameBitCount1 = minTerms[0].OrderBy(x => x.DoesntMatter).ThenBy(x => x.Bits.Value).ToArray(), sameBitCount0 = null;
                for (var sameBitCountIdx = 1; sameBitCountIdx < minTerms.Length; sameBitCountIdx++) {
                    sameBitCount0 = sameBitCount1;
                    sameBitCount1 = minTerms[sameBitCountIdx].OrderBy(x => x.DoesntMatter).ThenBy(x => x.Bits.Value).ToArray();
                    for (int i = 0; i < sameBitCount0.Length; i++) {
                        for (int j = 0; j < sameBitCount1.Length; j++) {
                            var minTerm0 = sameBitCount0[i];
                            var minTerm1 = sameBitCount1[j];
                            if (minTerm0.DoesntMatter == minTerm1.DoesntMatter) {
                                var diff = new Bits(minTerm0.Bits.Value ^ minTerm1.Bits.Value);
                                if (diff.BitCount == 1) {
                                    var combinedBits = minTerm0.Bits.Value & minTerm1.Bits.Value;
                                    var combinedDoesntMatter = minTerm0.DoesntMatter | diff.Value;
                                    var combinedCovers = minTerm0.Covers.Concat(minTerm1.Covers).OrderBy(x => x).ToArray();
                                    var combined = new Implicant(new Bits(combinedBits), combinedDoesntMatter, combinedCovers);
                                    nextStageImplicants.Add(combined);
                                    sameBitCount0[i].Combined = true;
                                    sameBitCount1[j].Combined = true;
                                }
                            }
                        }
                    }
                    primeImplicants.AddRange(sameBitCount0.Where(x => !x.Combined));
                }
                primeImplicants.AddRange(sameBitCount1.Where(x => !x.Combined));
            }

            primeImplicants = primeImplicants.OrderBy(x => x.Bits.Value).ThenBy(x => x.DoesntMatter).ToList();

            var requiredMinTerms = truthTable
                .Select((x, i) => new { x, i })
                .Where(x => x.x.HasValue && x.x.Value)
                .Select(x => x.i)
                .ToArray();

            var essentialPrimeImplicants = new HashSet<Implicant>();
            for (int i = 0; i < requiredMinTerms.Length; i++) {
                var requiredMinTerm = requiredMinTerms[i];
                var covers = primeImplicants.Where(x => x.Covers.Contains(requiredMinTerm)).ToArray();
                if (covers.Length == 1) {
                    essentialPrimeImplicants.Add(covers[0]);
                }
            }

            var requiredPrimeImplicants = essentialPrimeImplicants.ToArray();

            var nonEssentialMinTerms = requiredMinTerms.Where(x => !essentialPrimeImplicants.Any(y => y.Covers.Contains(x))).ToArray();
            var nonEssentialPrimeImplicants = primeImplicants.Except(essentialPrimeImplicants).ToArray();
            if (nonEssentialMinTerms.Any() && nonEssentialPrimeImplicants.Any()) {
                var extraPrimeImplicants = FindOptimalImplicants(nonEssentialPrimeImplicants, nonEssentialMinTerms);
                requiredPrimeImplicants = requiredPrimeImplicants.Concat(extraPrimeImplicants).ToArray();
            }

            var ctx = e.Ctx;
            Expr eResult;
            if (!requiredPrimeImplicants.Any()) {
                eResult = ctx.Literal(false);
            } else {
                var requiredAndExprs = requiredPrimeImplicants
                    .Select(x => {
                        var toOr = Enumerable.Range(0, exprs.Length).Select(i => {
                            var mask = 1 << i;
                            if ((x.DoesntMatter & mask) != 0) {
                                return null;
                            }
                            var needNot = (x.Bits.Value & mask) == 0;
                            if (needNot) {
                                return ctx.ExprGen.Not(exprs[i]);
                            } else {
                                return exprs[i];
                            }
                        })
                        .Where(y => y != null)
                        .ToArray();
                        if (toOr.Any()) {
                            var orExpr = toOr.Aggregate((a, b) => ctx.ExprGen.And(a, b));
                            return orExpr;
                        } else {
                            return ctx.Literal(true);
                        }
                    })
                    .ToArray();
                eResult = requiredAndExprs.Aggregate((a, b) => ctx.ExprGen.Or(a, b));
            }

            if (e.DoesEqual(eResult)) {
                return e;
            } else {
                return eResult;
            }
        }

        private static IEnumerable<Implicant> FindOptimalImplicants(Implicant[] primeImplicants, int[] requiredMinTerms) {
            // TODO: Use Petrick's method
            // But until then this simpler method will suffice

            var needMinTerms = requiredMinTerms.ToArray();
            var unusedImplicants = primeImplicants.ToList();

            var ret = new List<Implicant>();
            while (needMinTerms.Any()) {
                var nextImplicant = unusedImplicants
                    .Select(x => new { count = x.Covers.Intersect(needMinTerms).Count(), imp = x })
                    .OrderByDescending(x => x.count)
                    .First().imp;
                ret.Add(nextImplicant);
                needMinTerms = needMinTerms.Except(nextImplicant.Covers).ToArray();
                unusedImplicants.Remove(nextImplicant);
            }
            return ret;

            // TODO: Get Petrick's method working at a reasonable speed...
            //if (primeImplicants.Length > 64) {
            //    throw new NotSupportedException("Too many prime implicants (can fix by using BigInt rather than ulong)");
            //}
            //var piMap = primeImplicants.Select((x, i) => new { x, i }).ToDictionary(x => x.x, x => x.i);
            //var p = requiredMinTerms.Select(x =>
            //    primeImplicants.Where(y => y.Covers.Contains(x)).Aggregate((ulong)0, (acc, y) => acc | (1UL << piMap[y])))
            //    .ToArray();
            //Func<ulong[], ulong[]> switchForm = null;
            //switchForm = x => {
            //    var a = x[0];
            //    var ret = new List<ulong>();
            //    if (x.Length == 1) {
            //        ulong bitMask = 1;
            //        while (a > 0) {
            //            if ((a & 1) != 0) {
            //                ret.Add(bitMask);
            //            }
            //            a >>= 1;
            //            bitMask <<= 1;
            //        }
            //    } else {
            //        var xs = x.Skip(1).ToArray();
            //        var rest = switchForm(xs);
            //        ulong bitMask = 1;
            //        while (a > 0) {
            //            if ((a & 1) != 0) {
            //                ret.AddRange(rest.Select(y => y | bitMask));
            //            }
            //            a >>= 1;
            //            bitMask <<= 1;
            //        }
            //        var retOrdered = ret.OrderBy(y => y).ToArray();
            //        ret.Clear();
            //        ulong prev = retOrdered[0];
            //        foreach (var y in retOrdered.Skip(1)) {
            //            if (y != prev) {
            //                ret.Add(prev);
            //                prev = y;
            //            }
            //        }
            //        if (ret.Count == 0 || ret.Last() != prev) {
            //            ret.Add(prev);
            //        }
            //    start:
            //        for (int i = 0; i < ret.Count; i++) {
            //            for (int j = 0; j < ret.Count; j++) {
            //                if (i != j) {
            //                    var and = ret[i] & ret[j];
            //                    if (and == ret[i]) {
            //                        ret.RemoveAt(j);
            //                        if (j >= i) {
            //                            j--;
            //                        }
            //                        i--;
            //                        //goto start;
            //                    } else if (and == ret[j]) {
            //                        ret.RemoveAt(i);
            //                        if (i >= j) {
            //                            i--;
            //                        }
            //                        j--;
            //                        //goto start;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    return ret.ToArray();
            //};
            //var sumOfProducts = switchForm(p);

            //Func<ulong, int> bitCount = a => {
            //    int ret = 0;
            //    while (a > 0) {
            //        if ((a & 1) != 0) {
            //            ret++;
            //        }
            //        a >>= 1;
            //    }
            //    return ret;
            //};
            //var sopOrdered = sumOfProducts.OrderBy(x => bitCount(x)).ToArray();
            //var bc0 = bitCount(sopOrdered[0]);
            //var candidates = sopOrdered.TakeWhile(x => bitCount(x) == bc0).ToArray();

            //Func<ulong, int> literalCount = a => {
            //    int ret = 0;
            //    int bit = 0;
            //    while (a > 0) {
            //        if ((a & 1) != 0) {
            //            ret += primeImplicants[bit].Covers.Length;
            //        }
            //        a >>= 1;
            //        bit++;
            //    }
            //    return ret;
            //};
            //var candidatesOrdered = candidates.OrderBy(x => literalCount(x)).ToArray();
            //var use = candidatesOrdered[0];

            //var pmRet = new List<Implicant>();
            //int useBit = 0;
            //while (use > 0) {
            //    if ((use & 1) != 0) {
            //        pmRet.Add(primeImplicants[useBit]);
            //    }
            //    use >>= 1;
            //    useBit++;
            //}
            //return pmRet;
        }

    }
}
