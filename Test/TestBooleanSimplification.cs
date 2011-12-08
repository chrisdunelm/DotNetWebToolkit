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

    [TestFixture]
    public class TestBooleanSimplification {

        private static MethodDefinition methodCache = null;
        private static MethodDefinition GetMethod() {
            if (methodCache == null) {
                var mod = AssemblyDefinition.ReadAssembly(System.Reflection.Assembly.GetExecutingAssembly().Location).MainModule;
                var curMethod = System.Reflection.MethodBase.GetCurrentMethod();
                var type = mod.GetType(curMethod.DeclaringType.FullName);
                methodCache = type.Methods.First(x => x.Name == curMethod.Name);
            }
            return methodCache;
        }

        private static TypeSystem GetTypeSystem() {
            return GetMethod().Module.TypeSystem;
        }

        private static Expr V(ICode c) {
            return (Expr)VisitorBooleanSimplification.V(GetMethod(), c);
        }

        private static TypeReference booleanCache = null;
        private static TypeReference Boolean {
            get {
                if (booleanCache == null) {
                    booleanCache = new TypeDefinition("System", "Boolean", TypeAttributes.Class);
                }
                return booleanCache;
            }
        }

        private static ExprLiteral GetTrue() {
            return new ExprLiteral(true, Boolean);
        }

        private static ExprLiteral GetFalse() {
            return new ExprLiteral(false, Boolean);
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
            var eGen = Expr.ExprGen(GetTypeSystem());
            ICode e;
            var c = new ExprVarLocal(Boolean, "c");

            e = V(eGen.Not(eGen.Not(c)));
            Assert.That(e, Is.EqualTo(c));
        }

        [Test]
        public void TestSimple() {
            var eGen = Expr.ExprGen(GetTypeSystem());
            var c = new ExprVarLocal(Boolean, "c");

            AssertPerms(eGen.Or(GetFalse(), c), Is.EqualTo(c));
            AssertPerms(eGen.Or(GetTrue(), c), Is.EqualTo(GetTrue()).Using(Expr.EqComparer));
            AssertPerms(eGen.Or(c, c), Is.EqualTo(c));
            AssertPerms(eGen.Or(c, eGen.Not(c)), Is.EqualTo(GetTrue()).Using(Expr.EqComparer));

            AssertPerms(eGen.And(GetTrue(), c), Is.EqualTo(c));
            AssertPerms(eGen.And(GetFalse(), c), Is.EqualTo(GetFalse()).Using(Expr.EqComparer));
            AssertPerms(eGen.And(c, c), Is.EqualTo(c));
            AssertPerms(eGen.And(c, eGen.Not(c)), Is.EqualTo(GetFalse()).Using(Expr.EqComparer));
        }

        [Test]
        public void TestDeMorgans() {
            var eGen = Expr.ExprGen(GetTypeSystem());
            Expr eOrg;
            var a = new ExprVarLocal(Boolean, "a");
            var b = new ExprVarLocal(Boolean, "b");

            // !A + !B => !(AB)
            AssertPerms(eGen.Or(eGen.Not(a), eGen.Not(b)), Is.EqualTo(eGen.Not(eGen.And(a, b))).Using(Expr.EqComparer));
            // (!A)(!B) => !(A + B)
            AssertPerms(eGen.And(eGen.Not(a), eGen.Not(b)), Is.EqualTo(eGen.Not(eGen.Or(a, b))).Using(Expr.EqComparer));

            // Check it doesn't transform in the other direction
            // !(AB) !=> !A + !B
            AssertPerms(eOrg = eGen.Not(eGen.And(a, b)), Is.EqualTo(eOrg).Using(Expr.EqComparer));
            // !(A + B) !=> (!A)(!B)
            AssertPerms(eOrg = eGen.Not(eGen.Or(a, b)), Is.EqualTo(eOrg).Using(Expr.EqComparer));

        }

        [Test]
        public void TestAbsorption() {
            var eGen = Expr.ExprGen(GetTypeSystem());
            var a = new ExprVarLocal(Boolean, "a");
            var b = new ExprVarLocal(Boolean, "b");

            // A + AB = A
            AssertPerms(eGen.Or(a, eGen.And(a, b)), Is.EqualTo(a));
            // A(A + B) = A
            AssertPerms(eGen.And(a, eGen.Or(a, b)), Is.EqualTo(a));
        }

        [Test]
        public void TestDistribution() {
            var eGen = Expr.ExprGen(GetTypeSystem());
            var a = new ExprVarLocal(Boolean, "a");
            var b = new ExprVarLocal(Boolean, "b");
            var c = new ExprVarLocal(Boolean, "c");

            // (A + B)(A + C) = A + BC
            AssertPerms(eGen.And(eGen.Or(a, b), eGen.Or(a, c)), Is.EqualTo(eGen.Or(a, eGen.And(b, c))).Using(Expr.EqComparer));
            // AB + AC = A(B + C)
            AssertPerms(eGen.Or(eGen.And(a, b), eGen.And(a, c)), Is.EqualTo(eGen.And(a, eGen.Or(b, c))).Using(Expr.EqComparer));
        }

        [Test]
        public void TestSimplification() {
            var eGen = Expr.ExprGen(GetTypeSystem());
            var a = new ExprVarLocal(Boolean, "a");
            var b = new ExprVarLocal(Boolean, "b");
            //var c = new ExprVarLocal(Boolean, "c");

            // A + !AB = A + B
            AssertPerms(eGen.Or(a, eGen.And(eGen.Not(a), b)), Is.EqualTo(eGen.Or(a, b)).Using(Expr.EqComparer));
            // A((!A) + B) => AB
            AssertPerms(eGen.And(a, eGen.Or(eGen.Not(a), b)), Is.EqualTo(eGen.And(a, b)).Using(Expr.EqComparer));

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
