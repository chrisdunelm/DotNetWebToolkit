using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("JSON")]
    public class Json {

        private Json() { }

        public extern string Stringify(object obj);
        public extern object Parse(string json);

    }

}
