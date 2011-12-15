using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Test.Utils {
    static class CecilHelper {

        private static object lockObj = new object();

        private static ModuleDefinition module = null;
        public static ModuleDefinition GetModule() {
            lock (lockObj) {
                if (module == null) {
                    var filename = Assembly.GetExecutingAssembly().Location;
                    module = ModuleDefinition.ReadModule(filename);
                }
                return module;
            }
        }

        public static TypeDefinition GetType(Type type) {
            var module = GetModule();
            var name = type.FullName.Replace('+', '/');
            return module.GetType(name);
        }

        public static MethodDefinition GetMethod(MethodInfo methodInfo) {
            var type = GetType(methodInfo.DeclaringType);
            // Doesn't handle lots of things. E.g. overloading
            var method = type.Methods.First(x => x.Name == methodInfo.Name);
            return method;
        }

        public static MethodDefinition GetMethod(Delegate d) {
            return GetMethod(d.Method);
        }

    }
}
