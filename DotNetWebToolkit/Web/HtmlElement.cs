using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626

namespace DotNetWebToolkit.Web {

    [JsAbstractClass]
    public abstract class HtmlElement {

        public extern int Width { get; set; }
        public extern int Height { get; set; }

    }
}
