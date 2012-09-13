using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Attributes {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class JsReturnTypeDeepUseAttribute : Attribute {
    }

}
