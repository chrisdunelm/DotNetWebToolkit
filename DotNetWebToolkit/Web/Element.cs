using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsAbstractClass]
    public abstract class Element : Node {

        [JsDetail(Name = "innerHTML")]
        public extern string InnerHtml { get; set; }
        [JsDetail(Name = "outerHTML")]
        public extern string OuterHtml { get; set; }

        [JsDetail(IsDomEvent = true)]
        public extern Action<Event> OnClick { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action<Event> OnLoad { set; }

    }

}
