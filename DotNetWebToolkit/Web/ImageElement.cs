using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("IMG")]
    public class ImageElement : Element {

        [JsDetail(Name = "Image")]
        public extern ImageElement();
        [JsDetail(Name = "Image")]
        public extern ImageElement(uint width);
        [JsDetail(Name = "Image")]
        public extern ImageElement(uint width, uint height);

        public extern string Alt { get; set; }
        public extern bool Complete { get; }
        public extern string CrossOrigin { get; set; }
        public extern uint Height { get; set; }
        public extern bool IsMap { get; set; }
        public extern uint NaturalHeight { get; }
        public extern uint NaturalWidth { get; }
        public extern string Src { get; set; }
        public extern string UseMap { get; set; }
        public extern uint Width { get; set; }

    }

}
