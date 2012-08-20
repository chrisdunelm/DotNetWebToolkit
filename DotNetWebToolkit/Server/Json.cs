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
            var todo = new Queue<object>();
            todo.Enqueue(obj);
            var objIds = new Dictionary<object, string>();
            objIds.Add(obj, "0");
            int id = 0;
            Func<object, object> enc = o => {
                if (o == null) {
                    return null;
                }
                var type = o.GetType();
                var jsTypeName = this.typeMap.GetTypeName(type);
                if (jsTypeName == null) {
                    return new object[] { null, null };
                }
                var typeCode = Type.GetTypeCode(type);
                object jsO;
                switch (typeCode) {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.String:
                case TypeCode.Single:
                case TypeCode.Double:
                    jsO = o;
                    break;
                case TypeCode.Char:
                    jsO = (int)(char)o;
                    break;
                default:
                    throw new InvalidOperationException("Cannot handle: " + typeCode);
                }
                return new object[] { jsTypeName, jsO };
            };
            var res = new List<object>();
            while (todo.Count > 0) {
                var o = todo.Dequeue();
                var encoded = enc(o);
                var oId = objIds[o];
                res.Add(new object[] { oId, encoded });
            }
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var json = jsonSerializer.Serialize(res);
            return json;
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
            var arrayRefs = new List<Tuple<Array, int, string>>();
            Func<Type, object, object> jDecodeAny = null;
            Func<object, string> getIfObjRef = o => {
                var oArray = o as object[];
                if (oArray != null) {
                    if (oArray.Length == 1) {
                        return (string)oArray[0];
                    }
                }
                return null;
            };
            Func<Type, object[], object> createObj = (type, fields) => {
                var obj = Activator.CreateInstance(type, true);
                for (int i = 0; i < fields.Length; i += 2) {
                    var fieldInfo = this.typeMap.GetFieldByName(type, (string)fields[i]);
                    var fieldType = fieldInfo.FieldType;
                    var refId = getIfObjRef(fields[i + 1]);
                    if (refId != null) {
                        refs.Add(Tuple.Create(obj, fieldInfo, refId));
                    } else {
                        object v = jDecodeAny(fieldType, fields[i + 1]);
                        fieldInfo.SetValue(obj, v);
                    }
                }
                return obj;
            };
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
                case TypeCode.Int64:
                    var oi64Array = (object[])o;
                    var i64hi = Convert.ToUInt64(oi64Array[0]);
                    var i64lo = Convert.ToUInt64(oi64Array[1]);
                    var i64 = (i64hi << 32) | i64lo;
                    return (Int64)i64;
                case TypeCode.UInt64:
                    var oui64Array = (object[])o;
                    var ui64hi = Convert.ToUInt64(oui64Array[0]);
                    var ui64lo = Convert.ToUInt64(oui64Array[1]);
                    var ui64 = (ui64hi << 32) | ui64lo;
                    return ui64;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.Single:
                case TypeCode.Double:
                    return Convert.ChangeType(o, type);
                case TypeCode.Object:
                case TypeCode.DateTime:
                    var fields = (object[])o;
                    return createObj(type, fields);
                default:
                    throw new InvalidOperationException("Cannot handle: " + typeCode);
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
                    if (type.IsValueType) { // value-type
                        return jDecodeValueType(type, oArray[1]);
                    } else if (type.IsArray) { // array
                        var elementType = type.GetElementType();
                        var elements = (object[])oArray[1];
                        var array = (Array)Activator.CreateInstance(type, elements.Length);
                        for (int i = 0; i < elements.Length; i++) {
                            var refId = getIfObjRef(elements[i]);
                            if (refId != null) {
                                arrayRefs.Add(Tuple.Create(array, i, refId));
                            } else {
                                object v = jDecodeAny(elementType, elements[i]);
                                array.SetValue(v, i);
                            }
                        }
                        return array;
                    } else { // object
                        var fields = (object[])oArray[1];
                        return createObj(type, fields);
                    }
                }
            };
            jDecodeAny = (type, o) => {
                if (type != null && type.IsValueType) {
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
            foreach (var r in arrayRefs) {
                r.Item1.SetValue(objs[r.Item3], r.Item2);
            }
            var toRet = (T)objs["0"];
            return toRet;
        }

    }
}
