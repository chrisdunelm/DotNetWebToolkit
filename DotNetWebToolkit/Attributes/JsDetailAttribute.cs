using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Attributes {
    public class JsDetailAttribute : Attribute {

        public string Name { get; set; }
        public bool IsDomEvent { get; set; }

    }
}
