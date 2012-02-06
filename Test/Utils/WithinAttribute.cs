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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class WithinUlpsAttribute : Attribute {

        public WithinUlpsAttribute(int ulps) {
            this.Ulps = ulps;
        }

        public int Ulps { get; private set; }

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class WithinPercentAttribute : Attribute {

        public WithinPercentAttribute(double percent) {
            this.Percent = percent;
        }

        public double Percent { get; private set; }

    }

}
