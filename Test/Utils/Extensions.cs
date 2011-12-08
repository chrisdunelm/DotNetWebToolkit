using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Utils {
    static class Extensions {

        public static T C<T>(this object o) {
            return (T)o;
        }

    }
}
