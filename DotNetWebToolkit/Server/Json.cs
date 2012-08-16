using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DotNetWebToolkit.Server {
    public class Json {

        public Json(JsonTypeMap typeMap) {
            this.typeMap = typeMap;
        }

        private JsonTypeMap typeMap;

        public string Encode(object obj) {
            return "";
            var todo = new Queue<object>();
            var ids = new Dictionary<object, int>();
            int id = 0;
            var ret = new List<Tuple<int, string>>();

            todo.Enqueue(obj);
            ids.Add(obj, id++);

            var json = new StringBuilder();
            while (todo.Any()) {
                json.Clear();
                var o = todo.Dequeue();
                var oType = o.GetType();
                var typeName = this.typeMap.GetTypeName(oType);
                json.Append('[');
                if (typeName != null) {
                    json.Append('"');
                    json.Append(typeName);
                    json.Append('"');
                } else {
                    json.Append("null");
                }
                json.Append(", ");
                var fields = oType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields) {
                    var fieldName = this.typeMap.GetFieldName(oType, field);
                    if (fieldName == null) {
                        continue;
                    }
                    json.Append("[\"");
                    json.Append(fieldName);
                    json.Append("\", ");
                    var value = field.GetValue(o);
                    var fieldType = field.FieldType;
                    bool isNullable = false;
                    if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        isNullable = true;
                        fieldType = fieldType.GetGenericArguments()[0];
                    }
                    if (isNullable && value == null) {
                        json.Append("null");
                    } else {
                        var fieldTypeCode = Type.GetTypeCode(fieldType);
                        switch (fieldTypeCode) {
                        case TypeCode.Boolean:
                            json.Append((bool)value ? "true" : "false");
                            break;
                        case TypeCode.Char:
                            json.Append((int)(char)value);
                            break;
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            json.Append(value);
                            break;
                        case TypeCode.String:
                            json.Append('"');
                            foreach (var c in (string)value) {
                                if (c >= 32 && c < 128) {
                                    json.Append(c);
                                } else {
                                    json.Append("\\u");
                                    json.Append(((int)c).ToString("x4"));
                                }
                            }
                            json.Append('"');
                            break;
                        case TypeCode.Object:
                            if (value == null) {
                                json.Append("null");
                            } else {
                                int objId;
                                if (!ids.TryGetValue(value, out objId)) {
                                    objId = id++;
                                    ids.Add(value, objId);
                                    todo.Enqueue(value);
                                }
                                json.Append('"');
                                json.Append(objId);
                                json.Append("\", true");
                            }
                            break;
                        default:
                            throw new InvalidOperationException("Cannot handle: " + fieldTypeCode);
                        }
                    }
                    json.Append("], ");
                }
                json.Length -= 2;
                json.Append(']');
                var jsonId = ids[o];
                ret.Add(Tuple.Create(jsonId, json.ToString()));
            }

            var fullRet = string.Format("[{0}{1}{0}]", Environment.NewLine,
                string.Join("," + Environment.NewLine, ret.Select(x => string.Format("[\"{0}\", {1}]", x.Item1, x.Item2))));
            return fullRet;
        }

        public T Decode<T>(byte[] bytes) {
            var s = Encoding.UTF8.GetString(bytes);
            return Decode<T>(s);
        }

        public T Decode<T>(string s) {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var json = jsonSerializer.Deserialize<object[]>(s);
            var objs = new Dictionary<string, object>();
            var refs = new List<Tuple<object, FieldInfo, string>>();
            Func<Type, object, object> jDecodeAny = null;
            Func<Type, object, object> jDecodeValueType = (type, o) => {
                bool isNullable = false;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    isNullable = true;
                    type = type.GetGenericArguments()[0];
                }
                if (isNullable && o == null) {
                    return null;
                }
                var typeCode = Type.GetTypeCode(type);
                switch (typeCode) {
                case TypeCode.Boolean: return (bool)o;
                case TypeCode.Char: return (char)(int)o;
                case TypeCode.Int32: return (int)o;
                case TypeCode.Int64:
                    var oi64Array = (object[])o;
                    var i64hi = Convert.ToUInt64(oi64Array[0]);
                    var i64lo = Convert.ToUInt64(oi64Array[1]);
                    var i64 = (i64hi << 32) | i64lo;
                    return (Int64)i64;
                default: throw new InvalidOperationException("Cannot handle: " + type);
                }
            };
            Func<object, object> jDecode = null;
            jDecode = o => {
                if (o == null) {
                    return null;
                }
                var oStr = o as string;
                if (oStr != null) {
                    return oStr;
                } else {
                    var oArray = (object[])o;
                    var jsTypeName = (string)oArray[0];
                    var type = this.typeMap.GetTypeByName(jsTypeName);
                    if (type.IsValueType) {
                        return jDecodeValueType(type, oArray[1]);
                    } else if (type.IsArray) {
                        var elementType = type.GetElementType();
                        var elements = (object[])oArray[1];
                        var array = (Array)Activator.CreateInstance(type, elements.Length);
                        for (int i = 0; i < elements.Length; i++) {
                            object v = jDecodeAny(elementType, elements[i]);
                            array.SetValue(v, i);
                        }
                        return array;
                    } else { // object
                        var fields = (object[])oArray[1];
                        var obj = Activator.CreateInstance(type, true);
                        for (int i = 0; i < fields.Length; i += 2) {
                            var fieldInfo = this.typeMap.GetFieldByName(type, (string)fields[i]);
                            var fieldType = fieldInfo.FieldType;
                            if (!fieldType.IsValueType && fieldType != typeof(string)) {
                                // Object reference, stored as object id
                                var refId = (string)fields[i + 1];
                                if (refId != null) {
                                    refs.Add(Tuple.Create(obj, fieldInfo, refId));
                                } else {
                                    fieldInfo.SetValue(obj, null);
                                }
                            } else {
                                object v = jDecodeAny(fieldType, fields[i + 1]);
                                fieldInfo.SetValue(obj, v);
                            }
                        }
                        return obj;
                    }
                }
            };
            jDecodeAny = (type, o) => {
                if (type.IsValueType) {
                    return jDecodeValueType(type, o);
                } else {
                    return jDecode(o);
                }
            };
            foreach (var obj in json.Cast<object[]>()) {
                var objId = (string)obj[0];
                objs[objId] = jDecode(obj[1]);
            }
            foreach (var r in refs) {
                r.Item2.SetValue(r.Item1, objs[r.Item3]);
            }
            return (T)objs["0"];
        }

    }
}
