using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.WebGL;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("CANVAS")]
    public class HtmlCanvasElement : HtmlElement {

        private HtmlCanvasElement() { }

        public extern CanvasRenderingContext GetContext(string contextId);
        public extern CanvasRenderingContext GetContext(string contextId, object attrs);

    }

    public static class HtmlCanvasExtensions {

        public static CanvasRenderingContext2D GetContext2D(this HtmlCanvasElement canvas) {
            return (CanvasRenderingContext2D)canvas.GetContext("2d");
        }

        public static WebGLRenderingContext GetContextWebGL(this HtmlCanvasElement canvas, WebGLContextAttributes attrs = null) {
            return (WebGLRenderingContext)
                (canvas.GetContext("webgl", attrs) ??
                canvas.GetContext("experimental-webgl", attrs) ??
                canvas.GetContext("webkit-3d", attrs) ??
                canvas.GetContext("moz-webgl", attrs));
        }

    }

}
