using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Server;

namespace DotNetWebToolkit.Cil2Js.Output {
    public class JsResult {

        internal JsResult(string js, JsonTypeMap typeMap) {
            this.Js = js;
            this.TypeMap = typeMap;
        }

        public string Js { get; private set; }
        public JsonTypeMap TypeMap { get; private set; }

    }
}
