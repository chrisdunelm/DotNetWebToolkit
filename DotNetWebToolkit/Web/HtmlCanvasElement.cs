using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626

namespace DotNetWebToolkit.Web {

    public enum CanvasContext {
        TwoD,
        webGl,
    }

    [JsClass("CANVAS")]
    public class HtmlCanvasElement : HtmlElement {

        public extern int Width { get; set; }
        public extern int Height { get; set; }

        public extern CanvasRenderingContext GetContext(string contextId);

        public CanvasRenderingContext GetContext(CanvasContext context) {
            switch (context) {
            case CanvasContext.TwoD:
                return this.GetContext("2d");
            case CanvasContext.webGl:
                return this.GetContext("webgl") ?? this.GetContext("experimental-webgl");
            default:
                throw new ArgumentException("Invalid context");
            }
        }

    }

}
