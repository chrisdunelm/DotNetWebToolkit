using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestArrays : ExecutionTestBase {

        [Test]
        public void TestCreateSetGet1() {
            Func<int, int> f = a => {
                var array = new int[1];
                array[0] = a;
                return array[0];
            };
            this.Test(f);
        }

        [Test]
        public void TestCreateSetGetN() {
            Func<int, int, int> f = (a, b) => {
                var array = new int[a];
                for (int i = 0; i < a; i++) {
                    array[i] = b + i;
                }
                int r = 0;
                for (int i = 0; i < a; i++) {
                    r += array[i];
                }
                return r;
            };
            this.Test(f);
        }

        [Test]
        public void TestLength() {
            Func<int, int> f = a => {
                var array = new int[a];
                return array.Length;
            };
            this.Test(f);
        }

    }
}
