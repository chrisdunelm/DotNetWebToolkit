using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cil2Js.Ast;
using Mono.Cecil;
using Cil2Js.Analysis;
using NUnit.Framework.Constraints;
using Test.Utils;

namespace Test {

    [TestFixture, Ignore("Boolean simplification changed, these tests need rework")]
    public class TestBooleanSimplification : TestBase {

        private static Expr V(ICode c) {
            return (Expr)VisitorBooleanSimplification.V(c);
        }

        private static void AssertPerms(Expr expr, IResolveConstraint c) {
            var perms = VisitorCreatePerms.V(expr);
            foreach (var perm in perms) {
                var e = V(perm);
                Assert.That(e, c);
            }
        }

        [Test]
        public void TestNot() {
            var c = new ExprVarLocal(Ctx, Ctx.Boolean, "c");
            ICode e;

            e = V(ExprGen.Not(ExprGen.Not(c)));
            Assert.That(e, Is.EqualTo(c));
        }

        [Test]
        public void TestSimple() {
            var c = new ExprVarLocal(Ctx, Ctx.Boolean, "c");

            AssertPerms(ExprGen.Or(False, c), Is.EqualTo(c));
            AssertPerms(ExprGen.Or(True, c), Is.EqualTo(True).Using(Expr.EqComparer));
            AssertPerms(ExprGen.Or(c, c), Is.EqualTo(c));
            AssertPerms(ExprGen.Or(c, ExprGen.Not(c)), Is.EqualTo(True).Using(Expr.EqComparer));

            AssertPerms(ExprGen.And(True, c), Is.EqualTo(c));
            AssertPerms(ExprGen.And(False, c), Is.EqualTo(False).Using(Expr.EqComparer));
            AssertPerms(ExprGen.And(c, c), Is.EqualTo(c));
            AssertPerms(ExprGen.And(c, ExprGen.Not(c)), Is.EqualTo(False).Using(Expr.EqComparer));
        }

        [Test]
        public void TestDeMorgans() {
            Expr eOrg;
            var a = new ExprVarLocal(Ctx, Ctx.Boolean, "a");
            var b = new ExprVarLocal(Ctx, Ctx.Boolean, "b");

            // !A + !B => !(AB)
            AssertPerms(ExprGen.Or(ExprGen.Not(a), ExprGen.Not(b)), Is.EqualTo(ExprGen.Not(ExprGen.And(a, b))).Using(Expr.EqComparer));
            // (!A)(!B) => !(A + B)
            AssertPerms(ExprGen.And(ExprGen.Not(a), ExprGen.Not(b)), Is.EqualTo(ExprGen.Not(ExprGen.Or(a, b))).Using(Expr.EqComparer));

            // Check it doesn't transform in the other direction
            // !(AB) !=> !A + !B
            AssertPerms(eOrg = ExprGen.Not(ExprGen.And(a, b)), Is.EqualTo(eOrg).Using(Expr.EqComparer));
            // !(A + B) !=> (!A)(!B)
            AssertPerms(eOrg = ExprGen.Not(ExprGen.Or(a, b)), Is.EqualTo(eOrg).Using(Expr.EqComparer));

        }

        [Test]
        public void TestAbsorption() {
            var a = new ExprVarLocal(Ctx, Ctx.Boolean, "a");
            var b = new ExprVarLocal(Ctx, Ctx.Boolean, "b");

            // A + AB = A
            AssertPerms(ExprGen.Or(a, ExprGen.And(a, b)), Is.EqualTo(a));
            // A(A + B) = A
            AssertPerms(ExprGen.And(a, ExprGen.Or(a, b)), Is.EqualTo(a));
        }

        [Test]
        public void TestDistribution() {
            var a = new ExprVarLocal(Ctx, Ctx.Boolean, "a");
            var b = new ExprVarLocal(Ctx, Ctx.Boolean, "b");
            var c = new ExprVarLocal(Ctx, Ctx.Boolean, "c");

            // (A + B)(A + C) = A + BC
            AssertPerms(ExprGen.And(ExprGen.Or(a, b), ExprGen.Or(a, c)), Is.EqualTo(ExprGen.Or(a, ExprGen.And(b, c))).Using(Expr.EqComparer));
            // AB + AC = A(B + C)
            AssertPerms(ExprGen.Or(ExprGen.And(a, b), ExprGen.And(a, c)), Is.EqualTo(ExprGen.And(a, ExprGen.Or(b, c))).Using(Expr.EqComparer));
        }

        [Test]
        public void TestSimplification() {
            var a = new ExprVarLocal(Ctx, Ctx.Boolean, "a");
            var b = new ExprVarLocal(Ctx, Ctx.Boolean, "b");
            //var c = new ExprVarLocal(Boolean, "c");

            // A + !AB = A + B
            AssertPerms(ExprGen.Or(a, ExprGen.And(ExprGen.Not(a), b)), Is.EqualTo(ExprGen.Or(a, b)).Using(Expr.EqComparer));
            // A((!A) + B) => AB
            AssertPerms(ExprGen.And(a, ExprGen.Or(ExprGen.Not(a), b)), Is.EqualTo(ExprGen.And(a, b)).Using(Expr.EqComparer));
            // !(!AB) => A + !B
            AssertPerms(ExprGen.Not(ExprGen.And(ExprGen.Not(a), b)), Is.EqualTo(ExprGen.Or(a, ExprGen.Not(b))).Using(Expr.EqComparer));
            // !(!A + B) => A!B
            AssertPerms(ExprGen.Not(ExprGen.Or(ExprGen.Not(a), b)), Is.EqualTo(ExprGen.And(a, ExprGen.Not(b))).Using(Expr.EqComparer));

            // AB + AC + (!B)C => AB + (!B)C
            // Ignore for now, fairly obscure logic...
            //AssertPerms(Expr.Or(Expr.And(a, b), Expr.Or(Expr.And(a, c), Expr.And(Expr.Not(b), c))),
            //    Is.EqualTo(Expr.Or(Expr.And(a, b), Expr.And(Expr.Not(b), c))));
            //AssertPerms(Expr.Or(Expr.Or(Expr.And(a, b), Expr.And(a, c)), Expr.And(Expr.Not(b), c)),
            //    Is.EqualTo(Expr.Or(Expr.And(a, b), Expr.And(Expr.Not(b), c))));
            //AssertPerms(Expr.Or(Expr.Or(Expr.And(a, b), Expr.And(Expr.Not(b), c)), Expr.And(a, c)),
            //    Is.EqualTo(Expr.Or(Expr.And(a, b), Expr.And(Expr.Not(b), c))).Using(Expr.EqComparer));
        }

    }

}
