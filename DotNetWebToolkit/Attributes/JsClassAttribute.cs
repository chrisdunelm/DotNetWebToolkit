using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Attributes {

    [AttributeUsage(AttributeTargets.Class)]
    public class JsClassAttribute : Attribute {

        protected JsClassAttribute() {
            this.TagOrConstructorName = null;
        }

        public JsClassAttribute(string tagOrConstructorName) {
            this.TagOrConstructorName = tagOrConstructorName;
        }

        public string TagOrConstructorName { get; private set; }

    }

    public class JsAbstractClassAttribute : JsClassAttribute {
    }

}
