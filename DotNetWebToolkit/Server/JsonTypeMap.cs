using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Server {
    public class JsonTypeMap {

        public JsonTypeMap(Dictionary<Type, string> typeNames, Dictionary<Type, Dictionary<string, string>> fieldNames) {
            this.typeNames = typeNames;
            this.fieldNames = fieldNames;
            this.typesByName = this.typeNames.ToDictionary(x => x.Value, x => x.Key);
            this.fieldsByName = this.fieldNames.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Value, y => y.Key));
        }

        private Dictionary<Type, string> typeNames;
        private Dictionary<Type, Dictionary<string, string>> fieldNames;
        private Dictionary<string, Type> typesByName;
        private Dictionary<Type, Dictionary<string, string>> fieldsByName;

        public Type GetTypeByName(string jsName) {
            return this.typesByName[jsName];
            //var typeFullName = this.typesByName[jsName];
            //var type = Type.GetType(typeFullName);
            //return type;
        }

        public string GetTypeName(Type type) {
            string ret;
            //this.typeNames.TryGetValue(type.AssemblyQualifiedName, out ret);
            this.typeNames.TryGetValue(type, out ret);
            return ret;
        }

        public FieldInfo GetFieldByName(Type type, string jsName) {
            //var fields = this.fieldsByName[type.AssemblyQualifiedName];
            var fields = this.fieldsByName[type];
            var fieldName = fields[jsName];
            var f = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return f;
        }

        public string GetFieldName(Type type, FieldInfo field) {
            Dictionary<string, string> fields;
            //if (!this.fieldNames.TryGetValue(type.AssemblyQualifiedName, out fields)) {
            if (!this.fieldNames.TryGetValue(type, out fields)) {
                return null;
            }
            string ret;
            fields.TryGetValue(field.Name, out ret);
            return ret;
        }

    }
}
