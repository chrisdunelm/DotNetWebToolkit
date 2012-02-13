using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestEnumerable : ExecutionTestBase {

        private static IEnumerable<int> GetListOfInts() {
            yield return 4;
            yield return 2;
            yield return 8;
        }

        [Test]
        public void TestYield() {
            Func<int> f = () => {
                int sum = 0;
                foreach (var i in GetListOfInts()) {
                    sum += i;
                }
                return sum;
            };
            this.Test(f);
        }


    }

}
