using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    class JsAttribute : Attribute {

        public JsAttribute() {
            this.MethodName = null;
            this.ReturnType = null;
            this.Parameters = null;
        }

        public JsAttribute(string methodName, Type returnType, params Type[] parameters) {
            this.MethodName = methodName;
            this.ReturnType = returnType;
            this.Parameters = parameters;
        }

        public JsAttribute(Type returnType, params Type[] parameters) {
            this.MethodName = null;
            this.ReturnType = returnType;
            this.Parameters = parameters;
        }

        public JsAttribute(Type implType) {
            this.ImplType = implType;
            this.ReturnType = implType;
            this.Parameters = Enumerable.Empty<Type>();
        }

        public JsAttribute(string jsExplicit) {
            this.JsExplicit = jsExplicit;
        }

        public Type ImplType { get; private set; }
        public string JsExplicit { get; private set; }

        public string MethodName { get; private set; }
        public Type ReturnType { get; private set; }
        public IEnumerable<Type> Parameters { get; private set; }

    }

}
