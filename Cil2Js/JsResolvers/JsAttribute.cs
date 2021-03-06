﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Cil2Js.JsResolvers {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class JsAttribute : Attribute {

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

        public JsAttribute(Type returnTypeOrImplType, params Type[] parameters) {
            this.MethodName = null;
            this.ReturnType = returnTypeOrImplType;
            this.Parameters = parameters;
            this.ImplType = returnTypeOrImplType;
        }

        public JsAttribute(string jsExplicit) {
            this.JsExplicit = jsExplicit;
        }

        public Type ImplType { get; private set; }
        public string JsExplicit { get; private set; }

        public string MethodName { get; private set; }
        public Type ReturnType { get; private set; }
        public IEnumerable<Type> Parameters { get; private set; }

        public bool? IsStaticFull { get; private set; }
        public bool IsStatic {
            get { return this.IsStaticFull.GetValueOrDefault(); }
            set {
                this.IsStaticFull = value;
            }
        }

    }

}
