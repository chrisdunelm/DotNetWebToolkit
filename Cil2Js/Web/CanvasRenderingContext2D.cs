using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cil2Js.Attributes;

namespace Cil2Js.Web {

    [JsClass]
    public class CanvasRenderingContext2D : CanvasRenderingContext {

        public object FillStyle { get; set; }

        public void Rect(double x, double y, double w, double h) {
            throw new JsOnlyException();
        }

    }

}
