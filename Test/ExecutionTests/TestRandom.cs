using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    [ParamFullRange]
    public class TestRandom : ExecutionTestBase {

        [Test]
        public void TestR() {
            Func<int,int> f = a => {
                var rnd = new Random(0);
                return rnd.Next(0, Math.Abs(a & 0xf));
            };
            this.Test(f);
        }

    }

}
