using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Attributes {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class JsUseTypeAttribute : Attribute {

        public JsUseTypeAttribute(Type useType) {
            this.UseType = useType;
        }

        public Type UseType { get; private set; }

    }

}
