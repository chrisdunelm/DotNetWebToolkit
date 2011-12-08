using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FsCheck.Fluent;
using NUnit.Framework;

namespace Test.Utils {
    static class QCheck {

        public static void ForAny<T>(Func<T, bool> assertion) {
            Spec.ForAny<T>(xs => {
                var r = assertion(xs);
                Assert.That(r);
            }).QuickCheck();
        }

    }
}
