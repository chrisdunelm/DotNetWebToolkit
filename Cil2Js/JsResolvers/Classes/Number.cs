using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    sealed class Number {

        public static string FormatInt32(int value, string format, NumberFormatInfo info) {
            // TODO: Very basic, mostly broken
            string s = "";
            while (value > 0) {
                char c = (char)('0' + value % 10);
                s = c.ToString() + s;
                value /= 10;
            }
            return s;
        }

    }
}
