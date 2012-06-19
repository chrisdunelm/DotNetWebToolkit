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
        public void TestRandomInt32WithSeed() {
            Func<int, int> f = a => {
                var rnd = new Random(0);
                return rnd.Next(0, Math.Abs(a & 0xf));
            };
            this.Test(f);
        }

        [Test]
        public void TestRandomInt32NoSeed() {
            Func<bool> f = () => {
                const int sampleSize = 10000;
                const int numBuckets = 10;
                var buckets = new int[numBuckets];
                var rnd = new Random();
                for (int i = 0; i < sampleSize; i++) {
                    int r = rnd.Next(0, numBuckets);
                    buckets[r]++;
                }
                const int low = sampleSize / numBuckets - (sampleSize / 100);
                const int high = sampleSize / numBuckets + (sampleSize / 100);
                return buckets.All(x => x >= low && x <= high);
            };
            this.Test(f, true);
        }

    }

}
