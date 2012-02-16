using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626

namespace DotNetWebToolkit.Web {

    [JsClass("HTMLDocument")]
    public static class Document {

        public static extern HtmlBodyElement Body { get; }
        public static extern HtmlElement CreateElement(string tagName);
        public static extern HtmlElement GetElementById(string id);

    }

}
