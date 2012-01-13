using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cil2Js.Utils;
using Test.Utils;
using System.Reflection;

namespace Test {

    [TestFixture]
    class TestUtils {

        [Test, Ignore("Problem with FSharp.Core not being found")]
        public void TestCombine() {
            QCheck.ForAny<int[]>(xs => xs.Combine((a, b) => (int?)(a + b)).Sum() == xs.Sum());
            QCheck.ForAny<int[]>(xs => xs.Combine((a, b) => (int?)null).Sum() == xs.Sum());
            QCheck.ForAny<int[]>(xs => xs.Combine((a, b) => a < 0 ? (int?)null : (a + b)).Sum() == xs.Sum());
        }

        class A {
            public interface I1 {
                void M();
                void N();
            }
            public class B : I1 {
                public void M() { }
                void I1.N() { }
            }
        }

        //[Test, Ignore]
        //public void TestGetInterfaceMethodImplicit() {
        //    var type = CecilHelper.GetType(typeof(A.B));
        //    var iMethod = CecilHelper.GetMethod(typeof(A.I1).GetMethod("M"));
        //    var m = type.GetInterfaceMethod(iMethod);
        //    Assert.That(m, Is.EqualTo(CecilHelper.GetMethod(typeof(A.B).GetMethod("M"))));
        //}

        //[Test, Ignore]
        //public void TestGetInterfaceMethodExplicit() {
        //    var type = CecilHelper.GetType(typeof(A.B));
        //    var iMethod = CecilHelper.GetMethod(typeof(A.I1).GetMethod("N"));
        //    var m = type.GetInterfaceMethod(iMethod);
        //    Assert.That(m, Is.EqualTo(CecilHelper.GetMethod(typeof(A.B).GetMethod("Test.TestUtils.A.I1.N", BindingFlags.NonPublic | BindingFlags.Instance))));
        //}

    }

}
