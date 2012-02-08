using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestAsync : ExecutionTestBase {

        private static Task<int> GetInt() {
            return new Task<int>(() => 3);
        }

        [Test, Ignore("Async not yet supported")]
        public void Test1() {
            Func<int> f = () => {
                return Test1Func().Result;
            };
            this.Test(f);
        }
        private static async Task<int> Test1Func() {
            return await GetInt() + await GetInt();
        }
    }

}
