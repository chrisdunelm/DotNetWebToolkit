using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("CanvasRenderingContext2D")]
    public class CanvasRenderingContext2D : CanvasRenderingContext {

        private CanvasRenderingContext2D() { }

        public extern CanvasElement Canvas { get; }

        public extern void Save();
        public extern void Restore();

        public extern void Scale(double x, double y);
        public extern void Rotate(double angle);
        public extern void Translate(double x, double y);
        public extern void Transform(double a, double b, double c, double e, double f);

        public extern double GlobalAlpha { get; set; }
        public extern GlobalCompositeOperation GlobalCompositeOperation { get; set; }

        public extern object StrokeStyle { get; set; }
        public extern object FillStyle { get; set; }
        public extern CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1);
        public extern CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1);
        public extern CanvasPattern CreatePattern(ImageElement image, string repetition);
        public extern CanvasPattern CreatePattern(CanvasElement image, string repetition);
        public extern CanvasPattern CreatePattern(VideoElement image, string repetition);

        public extern double LineWidth { get; set; }
        public extern LineCap LineCap { get; set; }
        public extern LineJoin LineJoin { get; set; }
        public extern double MiterLimit { get; set; }

        public extern double ShadowOffsetX { get; set; }
        public extern double ShadowOffsetY { get; set; }
        public extern double ShadowBlur { get; set; }
        public extern string ShadowColor { get; set; }

        public extern void ClearRect(double x, double y, double w, double h);
        public extern void FillRect(double x, double y, double w, double h);
        public extern void StrokeRect(double x, double y, double w, double h);

        public extern void BeginPath();
        public extern void ClosePath();
        public extern void MoveTo(double x, double y);
        public extern void LineTo(double x, double y);
        public extern void QuadraticCurveTo(double cpx, double cpy, double x, double y);
        public extern void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);
        public extern void ArcTo(double x1, double y1, double x2, double y2, double radius);
        public extern void Rect(double x, double y, double w, double h);
        public extern void Arc(double x, double y, double radius, double startAngle, double EndAngle);
        public extern void Arc(double x, double y, double radius, double startAngle, double EndAngle, bool anticlockwise);
        public extern void Fill();
        public extern void Stroke();
        public extern void DrawSystemFocusRing(Element element);
        public extern bool DrawCustomFocusRing(Element element);
        public extern void ScrollPathIntoView();
        public extern void Clip();
        public extern bool IsPointInPath(double x, double y);

        public extern string Font { get; set; }
        public extern TextAlign TextAlign { get; set; }
        public extern TextBaseline TextBaseline { get; set; }
        public extern void FillText(string text, double x, double y);
        public extern void FillText(string text, double x, double y, double maxWidth);
        public extern void StrokeText(string text, double x, double y);
        public extern void StrokeText(string text, double x, double y, double maxWidth);
        public extern TextMetrics MeasureText(string text);

        public extern void DrawImage(ImageElement image, double dx, double dy);
        public extern void DrawImage(ImageElement image, double dx, double dy, double dw, double dh);
        public extern void DrawImage(ImageElement image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh);
        public extern void DrawImage(CanvasElement image, double dx, double dy);
        public extern void DrawImage(CanvasElement image, double dx, double dy, double dw, double dh);
        public extern void DrawImage(CanvasElement image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh);
        public extern void DrawImage(VideoElement image, double dx, double dy);
        public extern void DrawImage(VideoElement image, double dx, double dy, double dw, double dh);
        public extern void DrawImage(VideoElement image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh);

        public extern ImageData CreateImageData(double sw, double sh);
        public extern ImageData CreateImageData(ImageData image);
        public extern ImageData GetImageData(double sx, double sy, double sw, double sh);
        public extern void PutImageData(ImageData imageData, double dx, double dy);
        public extern void PutImageData(ImageData imageData, double dx, double dy, double dirtyX, double dirtyY, double dirtyWidth, double dirtyHeight);

    }

    [JsClass("CanvasGradient")]
    public class CanvasGradient {
        private CanvasGradient() { }
        public extern void AddColorStop(double offset, string color);
    }

    [JsClass("CanvasPattern")]
    public class CanvasPattern {
        private CanvasPattern() { }
    }

    [JsClass("TextMetrics")]
    public class TextMetrics {
        private TextMetrics() { }
        public extern double Width { get; }
    }

    [JsClass("ImageData")]
    public class ImageData {
        private ImageData() { }
        public extern uint Width { get; }
        public extern uint Height { get; }
        public extern UInt8ClampedArray Data { get; }
    }

    [JsStringEnum]
    public enum GlobalCompositeOperation {
        [JsDetail(Name = "source-atop")]
        SourceAtop,
        [JsDetail(Name = "source-in")]
        SourceIn,
        [JsDetail(Name = "source-out")]
        SourceOut,
        [JsDetail(Name = "source-over")]
        SourceOver,
        [JsDetail(Name = "destination-atop")]
        DestinationAtop,
        [JsDetail(Name = "destination-in")]
        DestinationIn,
        [JsDetail(Name = "destination-out")]
        DestinationOut,
        [JsDetail(Name = "destination-over")]
        DestinationOver,
        Lighter,
        Copy,
        Xor,
    }

    [JsStringEnum]
    public enum LineCap {
        Butt, Round, Square
    }

    [JsStringEnum]
    public enum LineJoin {
        Round, Bevel, Miter
    }

    [JsStringEnum]
    public enum TextAlign {
        Start, End, Left, Right, Center
    }

    [JsStringEnum]
    public enum TextBaseline {
        Top, Hanging, Middle, Alphabetic, Ideographic, Bottom
    }

    public static class CanvasRenderingContext2DExtensions {

        public static void FillText(this CanvasRenderingContext2D ctx, string text, double y) {
            var textWidth = ctx.MeasureText(text).Width;
            var x = (ctx.Canvas.Width - textWidth) / 2;
            ctx.FillText(text, x, y);
        }

    }

}
