using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetWebToolkit.Attributes {

    [AttributeUsage(AttributeTargets.Class)]
    public class JsExportAttribute : Attribute {

        public JsExportAttribute() {
            this.ExportName = null;
        }

        public JsExportAttribute(string exportName) {
            this.ExportName = exportName;
        }

        public string ExportName { get; private set; }

    }

}
