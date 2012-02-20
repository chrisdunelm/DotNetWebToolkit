using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsAbstractClass]
    public abstract class HtmlElement {

        public extern int Width { get; set; }
        public extern int Height { get; set; }

        [JsDetail(Name = "innerHTML")]
        public extern string InnerHtml { get; set; }

        public extern T AppendChild<T>(T child) where T : HtmlElement;

        [JsDetail(IsDomEvent = true)]
        public extern Action OnLoad { set; }

    }

    public static class HtmlElementExtensions {

        internal static void SafeSetEvent(this HtmlElement el, string evName, Action listener) {
        }

    }

}
