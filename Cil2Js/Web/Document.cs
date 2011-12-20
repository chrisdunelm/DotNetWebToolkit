using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Attributes;

namespace Cil2Js.Web {

    [JsClass]
    public static class Document {

        public static HtmlElement GetElementById(string id) {
            throw new JsOnlyException();
        }

    }

}
