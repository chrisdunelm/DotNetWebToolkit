using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using System.Reflection;
using DotNetWebToolkit.Cil2Js.Utils;

namespace Test {

    [TestFixture]
    public class TestCecilExtensions {

        class A {
        }

        class B<T, U> : A {
        }

        class C<X, Y, Z> : B<Z, X> {
        }

        class D<W> : C<int, bool, W> {
        }

        class E : D<double> {
        }

        class F<H> : E {
            public virtual void V1(H h) { }
            public virtual void V2<T>(H h, T t) { }
        }

        class G : F<float> {
            public virtual void V0(int i) { }
            public override void V1(float h) { }
            public override void V2<T>(float h, T t) { }
        }

        private ModuleDefinition moduleCache = null;
        private TypeReference GetType(string name) {
            if (this.moduleCache == null) {
                this.moduleCache = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location);
            }
            return this.moduleCache.GetType("Test.TestCecilExtensions/" + name);
        }

        private MethodReference GetMethod(string typeName, string methodName) {
            var type = this.GetType(typeName).Resolve();
            return type.Methods.First(x => x.Name == methodName);
        }

        private TypeSystem Ts { get { return this.moduleCache.TypeSystem; } }

        [Test]
        public void TestClassBases() {
            var g = this.GetType("G");
            Assert.That(g.FullName, Is.EqualTo("Test.TestCecilExtensions/G"));
            var f = g.GetBaseType();
            Assert.That(f.FullName, Is.EqualTo("Test.TestCecilExtensions/F`1<System.Single>"));
            var e = f.GetBaseType();
            Assert.That(e.FullName, Is.EqualTo("Test.TestCecilExtensions/E"));
            var d = e.GetBaseType();
            Assert.That(d.FullName, Is.EqualTo("Test.TestCecilExtensions/D`1<System.Double>"));
            var c = d.GetBaseType();
            Assert.That(c.FullName, Is.EqualTo("Test.TestCecilExtensions/C`3<System.Int32,System.Boolean,System.Double>"));
            var b = c.GetBaseType();
            Assert.That(b.FullName, Is.EqualTo("Test.TestCecilExtensions/B`2<System.Double,System.Int32>"));
            var a = b.GetBaseType();
            Assert.That(a.FullName, Is.EqualTo("Test.TestCecilExtensions/A"));
        }

        //[Test]
        //public void TestMethodBasesV0() {
        //    var m = this.GetMethod("G", "V0");
        //    var m0 = m.GetBaseMethodByType();
        //    Assert.That(m0, Is.Null);
        //}

        //[Test]
        //public void TestMethodBasesV1() {
        //    var gm = this.GetMethod("G", "V1");
        //    var hm = gm.GetBaseMethodByType();
        //    Assert.That(hm.FullName, Is.Null);
        //}

        [Test]
        public void TestMethodBasesV2() {
            var gm = this.GetMethod("G", "V2");
            var gmR = new MethodReference(gm.Name, gm.ReturnType, gm.DeclaringType);
            var gmT = new GenericInstanceMethod(gmR);
            gmT.GenericArguments.Add(this.Ts.Int32);
            gmT.Parameters.Add(gm.Parameters[0]);
            //gmT.Parameters.Add(new ParameterDefinition(gm.Parameters[1].GetResolvedType(gmT)));
            //var hm = gmT.GetBaseMethodByType();
            //Assert.That(hm.FullName, Is.Null);
        }

    }

}
