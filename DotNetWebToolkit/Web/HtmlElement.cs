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

        public extern T AppendChild<T>(T child) where T : HtmlElement;

        public extern void AddEventListener(string type, Action listener);

        [JsDetail(Name = "onload")]
        public extern Action OnLoad { get; set; }

    }
}
