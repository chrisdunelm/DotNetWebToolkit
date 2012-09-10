using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.Server {
    public class JsonTypeMap {

        public JsonTypeMap(Dictionary<string, string> typeNames, Dictionary<string, Dictionary<string, string>> fieldNames) {
            this.typeNames = typeNames;
            this.fieldNames = fieldNames;
            this.typesByName = this.typeNames.ToDictionary(x => x.Value, x => x.Key);
            this.fieldsByName = this.fieldNames.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Value, y => y.Key));
        }

        private Dictionary<string, string> typeNames;
        private Dictionary<string, Dictionary<string, string>> fieldNames;
        private Dictionary<string, string> typesByName;
        private Dictionary<string, Dictionary<string, string>> fieldsByName;

        public Type GetTypeByName(string jsName) {
            var name = this.typesByName[jsName];
            var type = Type.GetType(name);
            return type;
        }

        public string GetTypeName(Type type) {
            string ret;
            this.typeNames.TryGetValue(type.AssemblyQualifiedName, out ret);
            return ret;
        }

        public FieldInfo GetFieldByName(Type type, string jsName) {
            var fields = this.fieldsByName[type.AssemblyQualifiedName];
            var fieldName = fields[jsName];
            var f = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return f;
        }

        public string GetFieldName(Type type, FieldInfo field) {
            return this.GetFieldName(type, field.Name);
        }

        public string GetFieldName(Type type, string fieldName) {
            Dictionary<string, string> fields;
            if (!this.fieldNames.TryGetValue(type.AssemblyQualifiedName, out fields)) {
                return null;
            }
            string ret;
            fields.TryGetValue(fieldName, out ret);
            return ret;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (var typeName in this.typeNames) {
                sb.Append(typeName.Key);
                sb.Append(':');
                sb.Append(typeName.Value);
                sb.AppendLine();
            }
            sb.AppendLine();
            foreach (var fieldName in this.fieldNames) {
                sb.AppendLine(fieldName.Key);
                foreach (var name in fieldName.Value) {
                    sb.Append(' ');
                    sb.Append(name.Key);
                    sb.Append(':');
                    sb.Append(name.Value);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public static JsonTypeMap FromString(string s) {
            var lines = s.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var typeNames = new Dictionary<string, string>();
            var fieldNames = new Dictionary<string, Dictionary<string, string>>();
            int i;
            for (i = 0; lines[i] != ""; i++) {
                var line = lines[i];
                var data = line.Split(':');
                typeNames.Add(data[0], data[1]);
            }
            i++;
            while (i < lines.Length) {
                var type = lines[i];
                i++;
                var fields = new Dictionary<string, string>();
                while (i < lines.Length && lines[i].StartsWith(" ")) {
                    var data = lines[i].Substring(1).Split(':');
                    fields.Add(data[0], data[1]);
                    i++;
                }
                fieldNames.Add(type, fields);
            }
            return new JsonTypeMap(typeNames, fieldNames);
        }

    }
}
