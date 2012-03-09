using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Cil2Js.Analysis;
using DotNetWebToolkit.Cil2Js.Ast;
using Mono.Cecil;
using NUnit.Framework;
using Test.Categories;

namespace Test {

    [TestFixture, MiscTest]
    public class TestQuineMcCluskey {

        public bool Verbose { get; set; }

        private static void Method() { }
        private static Lazy<Ctx> _ctx = new Lazy<Ctx>(() => {
            var module = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location);
            var mRef = module.Import(((Action)Method).Method);
            var ctx = new Ctx(mRef.DeclaringType, mRef);
            return ctx;
        });
        private static Ctx ctx { get { return _ctx.Value; } }

        private static Expr NewVar(string name = null) {
            return new ExprVarLocal(ctx, ctx.Boolean, name);
        }

        private static readonly Expr a = NewVar("a");
        private static readonly Expr b = NewVar("b");
        private static readonly Expr c = NewVar("c");
        private static readonly Expr d = NewVar("d");

        private static Expr Not(Expr a) {
            return ctx.ExprGen.Not(a);
        }

        private static Expr And(params Expr[] es) {
            return es.Aggregate((a, b) => ctx.ExprGen.And(a, b));
        }

        private static Expr Or(params Expr[] es) {
            return es.Aggregate((a, b) => ctx.ExprGen.Or(a, b));
        }

        private static Expr QM(Expr e) {
            return VisitorBooleanSimplification.QuineMcCluskey(e, Enumerable.Empty<Expr>());
        }

        [Test]
        public void TestOr2() {
            var e = Or(a, b);
            var eSimp = QM(e);
            Assert.That(eSimp, Is.EqualTo(e));
        }

        [Test]
        public void TestAnd2() {
            var e = And(a, b);
            var eSimp = QM(e);
            Assert.That(eSimp, Is.EqualTo(e));
        }

        [Test]
        public void TestAandAB() {
            var e = And(a, Or(a, b));
            var eSimp = QM(e);
            Assert.That(eSimp, Is.EqualTo(a));
        }

        private static Expr BuildExpr(Expr[] vars, bool[] truthTable) {
            var vLen = vars.Length;
            var e = truthTable
                .Select((x, i) => new { x, i })
                .Where(x => x.x)
                .Select(x => x.i)
                .Aggregate((Expr)ctx.Literal(false), (acc, b) => {
                    var ands = new List<Expr>();
                    for (int i = 0, mask = 1; i < vLen; i++, mask <<= 1) {
                        if ((b & mask) != 0) {
                            // true
                            ands.Add(vars[i]);
                        } else {
                            // false
                            ands.Add(ctx.ExprGen.Not(vars[i]));
                        }
                    }
                    return ctx.ExprGen.Or(acc, ands.Aggregate((x, y) => ctx.ExprGen.And(x, y)));
                });
            return e;
        }

        private static void Verify(Expr e, Expr[] vars, bool[] truthTable) {
            var exprMap = Enumerable.Range(0, vars.Length).ToDictionary(i => vars[i], i => i);
            for (int i = 0; i < truthTable.Length; i++) {
                var bits = new VisitorBooleanSimplification.Bits(i);
                var calculated = VisitorBooleanSimplification.EvalVisitor.Eval(bits, exprMap, e);
                var expected = truthTable[i];
                Assert.That(calculated, Is.EqualTo(expected));
            }
        }

        [Test]
        public void TestRandomLots() {
            var rnd = new Random(0);
            for (int iteration = 0; iteration < 100; iteration++) {
                var numVars = rnd.Next(1, 9);
                var vars = Enumerable.Range(0, numVars).Select(i => NewVar(((char)('a' + i)).ToString())).ToArray();
                var ttSize = 1 << numVars;
                var truthTable = Enumerable.Range(0, ttSize).Select(x => rnd.Next(2) == 0).ToArray();
                var e = BuildExpr(vars, truthTable);
                var eSimp = QM(e);
                Verify(e, vars, truthTable);
            }
        }

    }

}
