using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Utils {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class IterationCountAttribute : Attribute {

        public IterationCountAttribute(int iterationCount) {
            this.IterationCount = iterationCount;
        }

        public int IterationCount { get; private set; }

    }

}
