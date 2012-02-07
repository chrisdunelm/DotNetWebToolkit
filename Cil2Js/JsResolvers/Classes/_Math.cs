using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Math {

        [Js("return Math.sqrt(a);")]
        public static double Sqrt(double value) {
            throw new Exception();
        }

    }
}
