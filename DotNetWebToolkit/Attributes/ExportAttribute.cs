using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Attributes {

    [AttributeUsage(AttributeTargets.Method)]
    public class ExportAttribute : Attribute {

        public ExportAttribute() {
            this.ExportName = null;
        }

        public ExportAttribute(string exportName) {
            this.ExportName = exportName;
        }

        public string ExportName { get; private set; }

    }

}
