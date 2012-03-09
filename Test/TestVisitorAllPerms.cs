using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetWebToolkit.Cil2Js.Ast;
using Test.Utils;
using Mono.Cecil;
using Test.Categories;

namespace Test {

    [TestFixture, MiscTest]
    public class TestVisitorAllPerms : TestBase {

        [Test]
        public void Test() {
            var exprGen = Ctx.ExprGen;
            Expr e;
            IEnumerable<Expr> perms;
            var a = new ExprVarLocal(Ctx, null, "a");
            var b = new ExprVarLocal(Ctx, null, "b");
            var c = new ExprVarLocal(Ctx, null, "c");
            var d = new ExprVarLocal(Ctx, null, "d");

            e = exprGen.And(a, b);
            perms = VisitorCreatePerms.V(e);
            Assert.That(perms, Is.Unique);
            Assert.That(perms, Is.EquivalentTo(
                new[] { exprGen.And(a, b), exprGen.And(b, a) }
                ).Using(Expr.EqComparerExact));

            e = exprGen.And(a, exprGen.Or(a, b));
            perms = VisitorCreatePerms.V(e);
            Assert.That(perms, Is.Unique);
            Assert.That(perms, Is.EquivalentTo(
                new[] {
                    exprGen.And(a, exprGen.Or(a, b)), exprGen.And(a, exprGen.Or(b, a)),
                    exprGen.And(exprGen.Or(a, b), a), exprGen.And(exprGen.Or(b, a), a)
                }
                ).Using(Expr.EqComparerExact));

            e = exprGen.And(exprGen.Or(a, b), exprGen.Or(c, d));
            perms = VisitorCreatePerms.V(e);
            Assert.That(perms, Is.Unique);
            Assert.That(perms, Is.EquivalentTo(
                new[]{
                    exprGen.And(exprGen.Or(a, b), exprGen.Or(c, d)), exprGen.And(exprGen.Or(c, d), exprGen.Or(a, b)),
                    exprGen.And(exprGen.Or(b, a), exprGen.Or(c, d)), exprGen.And(exprGen.Or(c, d), exprGen.Or(b, a)),
                    exprGen.And(exprGen.Or(a, b), exprGen.Or(d, c)), exprGen.And(exprGen.Or(d, c), exprGen.Or(a, b)),
                    exprGen.And(exprGen.Or(b, a), exprGen.Or(d, c)), exprGen.And(exprGen.Or(d, c), exprGen.Or(b, a)),
                }
                ).Using(Expr.EqComparerExact));
        }

    }

}
