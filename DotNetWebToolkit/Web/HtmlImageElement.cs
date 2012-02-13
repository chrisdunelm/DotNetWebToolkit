using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("IMG")]
    public class HtmlImageElement : HtmlElement {

        [JsDetail(Name = "Image")]
        public extern HtmlImageElement();

        public extern string Src { get; set; }

        [JsDetail(Name = "onload")]
        public extern Action OnLoad { get; set; }

    }

}
