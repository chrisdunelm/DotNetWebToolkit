﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test.Utils;

namespace Test.ExecutionTests {

    [TestFixture]
    public class TestList : ExecutionTestBase {

        [Test, Ignore("List<> not yet supported")]
        public void TestAddGetIndex() {
            Func<int, int, int, int> f = (a, b, c) => {
                var list = new List<int> { a, b, c };
                return list[0] + list[1] + list[2];
            };
            this.Test(f);
        }

    }

}
