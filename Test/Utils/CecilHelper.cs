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
            var module = GetModule();
            var method = module.Import(methodInfo).Resolve();
            return method;
        }

        public static MethodDefinition GetMethod(Delegate d) {
            return GetMethod(d.Method);
        }

    }
}
