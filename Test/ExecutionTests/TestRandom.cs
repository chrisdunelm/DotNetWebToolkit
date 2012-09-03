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
        [Ignore("Random class isn't quite identical - possibly due to int overflow differences")]
        public void TestRandomInt32WithSeed() {
            Func<int, int> f = a => {
                var rnd = new Random(a);
                return rnd.Next(0, 100) + rnd.Next(0, 10) + rnd.Next(-5, 5) + rnd.Next(20, 50);
            };
            this.Test(f);
        }

        [Test]
        [Ignore("Random class isn't quite identical - possibly due to int overflow differences")]
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

        [Test]
        [Ignore("Random class isn't quite identical - possibly due to int overflow differences")]
        public void TestRandomDoubleWithSeed() {
            Func<int, double> f = a => {
                var rnd = new Random(a);
                return rnd.NextDouble();
            };
            this.Test(f);
        }

    }

}
