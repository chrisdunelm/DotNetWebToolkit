using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Attributes;

#pragma warning disable 0626

namespace Cil2Js.Web {

    [JsClass]
    public static class Window {

        public static extern int SetInterval(Action func, int intervalMs);

    }

}
