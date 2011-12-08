using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cil2Js.Ast;
using Test.Utils;
using Mono.Cecil;

namespace Test {

    [TestFixture]
    public class TestVisitorAllPerms {

        [Test]
        public void Test() {
            var ts = AssemblyDefinition.ReadAssembly(System.Reflection.Assembly.GetExecutingAssembly().Location).MainModule.TypeSystem;
            var eGen = Expr.ExprGen(ts);
            Expr e;
            IEnumerable<Expr> perms;
            var a = new ExprVarLocal(null, "a");
            var b = new ExprVarLocal(null, "b");
            var c = new ExprVarLocal(null, "c");
            var d = new ExprVarLocal(null, "d");

            e = eGen.And(a, b);
            perms = VisitorCreatePerms.V(e);
            Assert.That(perms, Is.Unique);
            Assert.That(perms, Is.EquivalentTo(
                new[] { eGen.And(a, b), eGen.And(b, a) }
                ).Using(Expr.EqComparerExact));

            e = eGen.And(a, eGen.Or(a, b));
            perms = VisitorCreatePerms.V(e);
            Assert.That(perms, Is.Unique);
            Assert.That(perms, Is.EquivalentTo(
                new[] {
                    eGen.And(a, eGen.Or(a, b)), eGen.And(a, eGen.Or(b, a)),
                    eGen.And(eGen.Or(a, b), a), eGen.And(eGen.Or(b, a), a)
                }
                ).Using(Expr.EqComparerExact));

            e = eGen.And(eGen.Or(a, b), eGen.Or(c, d));
            perms = VisitorCreatePerms.V(e);
            Assert.That(perms, Is.Unique);
            Assert.That(perms, Is.EquivalentTo(
                new[]{
                    eGen.And(eGen.Or(a, b), eGen.Or(c, d)), eGen.And(eGen.Or(c, d), eGen.Or(a, b)),
                    eGen.And(eGen.Or(b, a), eGen.Or(c, d)), eGen.And(eGen.Or(c, d), eGen.Or(b, a)),
                    eGen.And(eGen.Or(a, b), eGen.Or(d, c)), eGen.And(eGen.Or(d, c), eGen.Or(a, b)),
                    eGen.And(eGen.Or(b, a), eGen.Or(d, c)), eGen.And(eGen.Or(d, c), eGen.Or(b, a)),
                }
                ).Using(Expr.EqComparerExact));
        }

    }

}
