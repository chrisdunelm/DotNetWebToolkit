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

        public CanvasRenderingContext2D GetContext2D() {
            return (CanvasRenderingContext2D)this.GetContext("2d");
        }

        public WebGLRenderingContext GetContextWebGL(WebGLContextAttributes attrs = null) {
            return (WebGLRenderingContext)
                (this.GetContext("webgl", attrs) ?? 
                this.GetContext("experimental-webgl", attrs) ??
                this.GetContext("webkit-3d", attrs) ??
                this.GetContext("moz-webgl", attrs));
        }

    }

}
