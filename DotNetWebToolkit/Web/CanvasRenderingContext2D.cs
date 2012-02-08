using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626

namespace DotNetWebToolkit.Web {

    [JsClass("CanvasRenderingContext2D")]
    public class CanvasRenderingContext2D : CanvasRenderingContext {

        public extern HtmlCanvasElement Canvas { get; }

        public extern object FillStyle { get; set; }

        public extern void FillRect(double x, double y, double w, double h);

        public extern string Font { get; set; }

        public extern void FillText(string text, double x, double y);
        public extern TextMetrics MeasureText(string text);

        public void FillText(string text, double y) {
            var textWidth = this.MeasureText(text).Width;
            var x = (this.Canvas.Width - textWidth)/2;
            this.FillText(text, x, y);
        }

    }

    [JsClass("TextMetrics")]
    public class TextMetrics {
        public extern double Width { get; }
    }

}
