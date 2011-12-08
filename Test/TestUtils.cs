using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cil2Js.Utils;
using Test.Utils;

namespace Test {

    [TestFixture]
    class TestUtils {

        [Test]
        public void TestCombine() {
            QCheck.ForAny<int[]>(xs => xs.Combine((a, b) => (int?)(a + b)).Sum() == xs.Sum());
            QCheck.ForAny<int[]>(xs => xs.Combine((a, b) => (int?)null).Sum() == xs.Sum());
            QCheck.ForAny<int[]>(xs => xs.Combine((a, b) => a < 0 ? (int?)null : (a + b)).Sum() == xs.Sum());
        }

    }

}
