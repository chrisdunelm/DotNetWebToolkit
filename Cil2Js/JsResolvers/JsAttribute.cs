using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    [AttributeUsage(AttributeTargets.Method)]
    class JsAttribute : Attribute {

        public JsAttribute(Type implType) {
            this.ImplType = implType;
        }

        public Type ImplType { get; private set; }

    }

}
