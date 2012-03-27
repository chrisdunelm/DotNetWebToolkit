using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.WebGL;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("CANVAS")]
    public class CanvasElement : Element {

        private CanvasElement() { }

        public extern int Height { get; set; }
        public extern int Width { get; set; }

        public extern CanvasRenderingContext GetContext(string contextId);
        public extern CanvasRenderingContext GetContext(string contextId, object attrs);

    }

    public static class CanvasExtensions {

        public static CanvasRenderingContext2D GetContext2D(this CanvasElement canvas) {
            return (CanvasRenderingContext2D)canvas.GetContext("2d");
        }

        public static WebGLRenderingContext GetContextWebGL(this CanvasElement canvas, WebGLContextAttributes attrs = null) {
            return (WebGLRenderingContext)
                (canvas.GetContext("webgl", attrs) ??
                canvas.GetContext("experimental-webgl", attrs) ??
                canvas.GetContext("webkit-3d", attrs) ??
                canvas.GetContext("moz-webgl", attrs));
        }

    }

}
