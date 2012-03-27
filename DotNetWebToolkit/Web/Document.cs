using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("HTMLDocument")]
    public class Document {

        private Document() { }

        public extern BodyElement Body { get; }
        public extern Element CreateElement(string tagName);
        public extern Element GetElementById(string id);

    }

}
