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
        [JsDetail(Name = "Image")]
        public extern HtmlImageElement(uint width);
        [JsDetail(Name = "Image")]
        public extern HtmlImageElement(uint width, uint height);

        public extern string Src { get; set; }
        public extern string Alt { get; set; }
        public extern uint NaturalWidth { get; }
        public extern uint NaturalHeight { get; }
        public extern bool Complete { get; }

    }

}
