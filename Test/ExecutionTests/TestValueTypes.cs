using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestValueTypes : ExecutionTestBase {

        struct S1 {
            public int x, y;
        }
        static int MutateS1(object s1) {
            var s1Typed = (S1)s1;
            s1Typed.x++;
            s1Typed.y--;
            return s1Typed.x + s1Typed.y;
        }

        [Test]
        public void TestPassStructAsObjectWithMutation() {
            Func<bool> f = () => {
                var s1 = new S1 { x = 0, y = 1 };
                var r = MutateS1(s1);
                return r == 1 && s1.x == 0 && s1.y == 1;
            };
            this.Test(f);
        }

        struct S2A {
            public int y;
        }
        struct S2 {
            public int x;
            public S2A s;
        }
        static int MutateS2(object s2) {
            var s2Typed = (S2)s2;
            s2Typed.x++;
            s2Typed.s.y++;
            return s2Typed.x + s2Typed.s.y;
        }

        [Test]
        public void TestPassNestedStructAsObjectWithMutation() {
            Func<int> f = () => {
                var s2 = new S2 { x = 1, s = new S2A { y = 2 } };
                var r = MutateS2(s2);
                return r + s2.x + s2.s.y;
            };
            this.Test(f);
        }

    }

}
