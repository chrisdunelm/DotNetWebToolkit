using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {
    class JsRedirectAttribute : Attribute {

        public JsRedirectAttribute(Type redirectToType = null) {
            this.RedirectToType = redirectToType;
        }

        public Type RedirectToType { get; private set; }

    }
}
