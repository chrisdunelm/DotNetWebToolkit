using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers.Classes {
    sealed class _Number {

        public static string FormatInt32(int value, string format, _NumberFormatInfo info) {
            // TODO: Very basic, mostly broken
            if (value == int.MinValue) {
                return "-2147483648";
            }
            if (value == 0) {
                return "0";
            }
            string s = "";
            bool isNeg = false;
            if (value < 0) {
                value = -value;
                isNeg = true;
            }
            while (value > 0) {
                char c = (char)('0' + value % 10);
                s = c.ToString() + s;
                value /= 10;
            }
            if (isNeg) {
                s = "-" + s;
            }
            return s;
        }

        public static string FormatDouble(double value, string format, _NumberFormatInfo info) {
            return "double!";
        }

    }
}
