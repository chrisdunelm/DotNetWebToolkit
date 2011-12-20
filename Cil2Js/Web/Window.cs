using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Attributes;

namespace Cil2Js.Web {

    [JsClass]
    public static class Window {

        public static int SetInterval(Action func, int intervalMs) {
            throw new JsOnlyException();
        }

    }

}
