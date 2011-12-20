using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Attributes;

namespace Cil2Js.Web {

    [JsClass]
    public class HtmlCanvasElement : HtmlElement {

        public int Width { get; set; }
        public int Height { get; set; }

        public CanvasRenderingContext GetContext(string contextId) {
            throw new JsOnlyException();
        }

    }

}
