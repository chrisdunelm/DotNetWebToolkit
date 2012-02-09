using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    class _Math {
        
        // TODO: Implement mechanism to allow this to be replaced at call site.
        // Although the (currently hypothetical) global optimiser should inline these

        [Js("return Math.sqrt(a);")]
        public static double Sqrt(double a) { throw new Exception(); }

        [Js("return Math.sin(a);")]
        public static double Sin(double a) { throw new Exception(); }

        [Js("return Math.cos(a);")]
        public static double Cos(double a) { throw new Exception(); }

        [Js("return Math.tan(a);")]
        public static double Tan(double a) { throw new Exception(); }

    }
}
