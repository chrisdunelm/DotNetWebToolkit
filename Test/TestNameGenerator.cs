using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Cil2Js.Output;
using NUnit.Framework;

namespace Test {

    [TestFixture]
    public class TestNameGenerator {

        [Test]
        public void TestGetMany() {
            var gen = new NameGenerator();
            var seen = new HashSet<string>();
            int prevLength = 0;
            for (int i = 0; i < 10000; i++) {
                var name = gen.GetNewName();
                Assert.That(seen.Contains(name), Is.False);
                Assert.That(name.Length, Is.GreaterThanOrEqualTo(prevLength));
                seen.Add(name);
                prevLength = name.Length;
            }
        }

    }

}
