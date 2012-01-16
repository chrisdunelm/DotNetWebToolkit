using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626

namespace DotNetWebToolkit.Web {

    [JsClass]
    public class CanvasRenderingContext2D : CanvasRenderingContext {

        public extern object FillStyle { get; set; }

        public extern void FillRect(double x, double y, double w, double h);

    }

}
