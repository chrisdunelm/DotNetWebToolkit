using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Utils {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class WithinAttribute : Attribute {

        public WithinAttribute(double delta) {
            this.Delta = delta;
        }

        public double Delta { get; private set; }

    }

}
