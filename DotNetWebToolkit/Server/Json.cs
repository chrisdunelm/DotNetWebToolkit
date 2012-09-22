using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DotNetWebToolkit.Server {
    public class Json {

        public static Json FromFile(string path) {
            var s = File.ReadAllText(path);
            var typeMap = JsonTypeMap.FromString(s);
            return new Json(typeMap);
        }

        public Json(JsonTypeMap typeMap) {
            this.typeMap = typeMap;
        }

        private JsonTypeMap typeMap;

        interface ICustomTypeCodec {
            bool IsType(Type type);
            object Encode(object o, Type type, JsonTypeMap typeMap, Func<object, bool, object> enc);
            object Decode(object o, Type type, JsonTypeMap typeMap, Action<Action<Dictionary<string, object>>> fnAdd, Func<object, Type, object> fnDecode);
        }

        class CustomListCodec : ICustomTypeCodec {
            public bool IsType(Type type) {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            }

            public object Encode(object o, Type type, JsonTypeMap typeMap, Func<object, bool, object> enc) {
                var oList = (IList)o;
                var listElementType = type.GetGenericArguments()[0];
                var innerArray = (Array)Activator.CreateInstance(listElementType.MakeArrayType(), oList.Count);
                int index = 0;
                foreach (var element in oList) {
                    innerArray.SetValue(element, index++);
                }
                var innerArrayEnc = enc(innerArray, false);
                var jsTypeName = typeMap.GetTypeName(type);
                var jsFieldName = typeMap.GetFieldName(type, "array");
                var ret = new Dictionary<string, object> {
                    { "_", jsTypeName },
                    { jsFieldName, innerArrayEnc }
                };
                return ret;
            }

            public object Decode(object o, Type type, JsonTypeMap typeMap, Action<Action<Dictionary<string, object>>> fnAdd, Func<object, Type, object> fnDecode) {
                var elementsId = (string)((object[])((Dictionary<string, object>)o).Where(x => x.Key != "_").First().Value)[0];
                var retList = (IList)Activator.CreateInstance(type);
                // Done in this slightly convoluted way, as the element Array must be fully populated
                // before the elements are added to the list.
                fnAdd(objs => {
                    fnAdd(objs2 => {
                        var elements = (Array)objs2[elementsId];
                        foreach (var element in elements) {
                            retList.Add(element);
                        }
                    });
                });
                return retList;
            }
        }

        class CustomDictionaryCodec : ICustomTypeCodec {

            public bool IsType(Type type) {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            }

            public object Encode(object o, Type type, JsonTypeMap typeMap, Func<object, bool, object> enc) {
                var oDict = (IDictionary)o;
                var jsTypeName = typeMap.GetTypeName(type);
                var comparerObj = type.InvokeMember("Comparer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty, null, o, new object[0]);
                var comparer = enc(comparerObj, false);
                var keys = new List<object>(oDict.Count);
                var values = new List<object>(oDict.Count);
                foreach (DictionaryEntry entry in oDict) {
                    var key = enc(entry.Key, false);
                    var value = enc(entry.Value, false);
                    keys.Add(key);
                    values.Add(value);
                }
                var ret = new Dictionary<string, object> {
                    { "_", jsTypeName },
                    { "__", 2 },
                    { "a", keys },
                    { "b", values },
                    { "c", comparer },
                    { "d", typeMap.GetTypeName(type) },
                };
                return ret;
            }

            public object Decode(object o, Type type, JsonTypeMap typeMap, Action<Action<Dictionary<string, object>>> fnAdd, Func<object, Type, object> fnDecode) {
                var retDict = (IDictionary)Activator.CreateInstance(type);
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var entries = (object[])((Dictionary<string, object>)o)["v"];
                for (int i = 0; i < entries.Length; i += 2) {
                    var key = fnDecode(entries[i], keyType);
                    var value = fnDecode(entries[i + 1], valueType);
                    if (key is DecodeRef || value is DecodeRef) {
                        fnAdd(objs => {
                            if (key is DecodeRef) {
                                key = objs[((DecodeRef)key).id];
                            }
                            if (value is DecodeRef) {
                                value = objs[((DecodeRef)value).id];
                            }
                            retDict.Add(key, value);
                        });
                    } else {
                        retDict.Add(key, value);
                    }
                }
                return retDict;
            }

        }

        private static ICustomTypeCodec[] customTypeCodecs = {
                                                                new CustomListCodec(),
                                                                new CustomDictionaryCodec(),
                                                            };

        public string Encode(object obj) {
            var todo = new Queue<object>();
            todo.Enqueue(obj);
            var objIds = new Dictionary<object, string>();
            if (obj != null) {
                objIds.Add(obj, "0");
            }
            int id = 0;
            Func<object, bool, bool, object> enc = null;
            enc = (o, inObject, isRoot) => {
                if (o == null) {
                    return null;
                }
                var type = o.GetType();
                var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                var typeCode = Type.GetTypeCode(isNullable ? type.GetGenericArguments()[0] : type);
                object ret;
                switch (typeCode) {
                case TypeCode.String:
                    return o;
                case TypeCode.Object:
                case TypeCode.DateTime:
                    // objects + structs
                    if (isRoot || type.IsValueType) {
                        ret = null;
                        foreach (var customTypeCodec in customTypeCodecs) {
                            if (customTypeCodec.IsType(type)) {
                                ret = customTypeCodec.Encode(o, type, this.typeMap, (toEnc, inObject2) => enc(toEnc, inObject2, false));
                                break;
                            }
                        }
                        if (ret == null) {
                            if (type.IsArray) {
                                var oArray = (Array)o;
                                var elType = type.GetElementType();
                                var retArray = new object[oArray.Length];
                                for (int i = 0; i < oArray.Length; i++) {
                                    retArray[i] = enc(oArray.GetValue(i), !elType.IsValueType, false);
                                }
                                ret = new Dictionary<string, object>{
                                    { "_", this.typeMap.GetTypeName(type) },
                                    { "__", 0 },
                                    { "", retArray },
                                };
                            } else {
                                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                var qFieldNamesValues =
                                    from field in fields
                                    let jsName = this.typeMap.GetFieldName(type, field)
                                    where jsName != null
                                    let value = field.GetValue(o)
                                    select new { jsName, value = enc(value, !field.FieldType.IsValueType, false) };
                                var retDict = qFieldNamesValues.ToDictionary(x => x.jsName, x => x.value);
                                if (!type.IsValueType) {
                                    retDict.Add("_", this.typeMap.GetTypeName(type));
                                }
                                ret = retDict;
                            }
                        }
                    } else {
                        string objId;
                        if (!objIds.TryGetValue(o, out objId)) {
                            todo.Enqueue(o);
                            objId = (++id).ToString();
                            objIds.Add(o, objId);
                        }
                        ret = new object[] { objId };
                    }
                    break;
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    ret = o;
                    break;
                case TypeCode.Single:
                    var oSingle = (Single)o;
                    if (Single.IsNaN(oSingle)) {
                        ret = new object[] { 0 };
                    } else if (Single.IsNegativeInfinity(oSingle)) {
                        ret = new object[] { -1 };
                    } else if (Single.IsPositiveInfinity(oSingle)) {
                        ret = new object[] { 1 };
                    } else {
                        ret = o;
                    }
                    break;
                case TypeCode.Double:
                    var oDouble = (Double)o;
                    if (Double.IsNaN(oDouble)) {
                        ret = new object[] { 0 };
                    } else if (Double.IsNegativeInfinity(oDouble)) {
                        ret = new object[] { -1 };
                    } else if (Double.IsPositiveInfinity(oDouble)) {
                        ret = new object[] { 1 };
                    } else {
                        ret = o;
                    }
                    break;
                case TypeCode.Char:
                    ret = (int)(char)o;
                    break;
                case TypeCode.Int64:
                    var i64 = (UInt64)(Int64)o;
                    ret = new object[] {
                        i64 >> 32,
                        i64 & 0xffffffff
                    };
                    break;
                case TypeCode.UInt64:
                    var ui64 = (UInt64)o;
                    ret = new object[]{
                        ui64 >> 32,
                        ui64 & 0xffffffff
                    };
                    break;
                default:
                    throw new InvalidOperationException("Cannot handle: " + typeCode);
                }
                if (inObject && type.IsValueType) {
                    return new Dictionary<string, object> {
                        { "_", this.typeMap.GetTypeName(type) },
                        { "v", ret }
                    };
                } else {
                    return ret;
                }
            };
            var res = new List<object>();
            bool isFirst = true;
            while (todo.Count > 0) {
                var o = todo.Dequeue();
                var encoded = enc(o, true, true);
                var oId = isFirst ? "0" : objIds[o];
                res.Add(new object[] { oId, encoded });
                isFirst = false;
            }
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var json = jsonSerializer.Serialize(res);
            return json;
        }

        public T Decode<T>(byte[] bytes) {
            var s = Encoding.UTF8.GetString(bytes);
            return Decode<T>(s);
        }

        private static bool IsNullable(Type type) {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static Type NullableType(Type type) {
            return IsNullable(type) ? type.GetGenericArguments()[0] : null;
        }

        class DecodeRef {
            public DecodeRef(string id) { this.id = id; }
            public string id;
        }

        public T Decode<T>(string s) {
            var jsonSerializer = new JavaScriptSerializer();
            var json = jsonSerializer.Deserialize<object[][]>(s);
            var objs = new Dictionary<string, object>();
            var refs = new List<Action>();
            Func<object, Type, object> decode = null;
            decode = (o, type) => {
                if (o == null) {
                    return null;
                }
                if (o is object[]) {
                    var oArray = (object[])o;
                    if (oArray.Length == 1 && oArray[0] is string) {
                        return new DecodeRef((string)((object[])o)[0]);
                    }
                }
                type = NullableType(type) ?? type;
                if (type != null && type.IsValueType) {
                    var typeCode = Type.GetTypeCode(type);
                    switch (typeCode) {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.Char:
                        return Convert.ChangeType(o, type);
                    case TypeCode.Int64:
                        var i64o = (object[])o;
                        var i64Hi = Convert.ToUInt64(i64o[0]);
                        var i64Lo = Convert.ToUInt64(i64o[1]);
                        return (Int64)((i64Hi << 32) | i64Lo);
                    case TypeCode.UInt64:
                        var u64o = (object[])o;
                        var u64Hi = Convert.ToUInt64(u64o[0]);
                        var u64Lo = Convert.ToUInt64(u64o[1]);
                        return (u64Hi << 32) | u64Lo;
                    case TypeCode.Single:
                        if (o.GetType().IsArray) {
                            switch ((int)((object[])o)[0]) {
                            case 0: return Single.NaN;
                            case -1: return Single.NegativeInfinity;
                            case 1: return Single.PositiveInfinity;
                            default: throw new InvalidOperationException("Unrecognised special Single");
                            }
                        }
                        return Convert.ToSingle(o);
                    case TypeCode.Double:
                        if (o.GetType().IsArray) {
                            switch ((int)((object[])o)[0]) {
                            case 0: return Double.NaN;
                            case -1: return Double.NegativeInfinity;
                            case 1: return Double.PositiveInfinity;
                            default: throw new InvalidOperationException("Unrecognised special Single");
                            }
                        }
                        return Convert.ToDouble(o);
                    case TypeCode.Object:
                        var oDictValueType = (Dictionary<string, object>)o;
                        var ret = Activator.CreateInstance(type, true);
                        foreach (var field in oDictValueType) {
                            var fieldInfo = this.typeMap.GetFieldByName(type, field.Key);
                            var fieldValue = decode(field.Value, fieldInfo.FieldType);
                            if (fieldValue is DecodeRef) {
                                var refId = ((DecodeRef)fieldValue).id;
                                refs.Add(() => fieldInfo.SetValue(ret, objs[refId]));
                            } else {
                                fieldInfo.SetValue(ret, fieldValue);
                            }
                        }
                        return ret;
                    default:
                        throw new InvalidOperationException("Cannot handle value-type: " + type);
                    }
                }
                if (o is string) {
                    return o;
                }
                var oDict = (Dictionary<string, object>)o;
                if (oDict.ContainsKey("_")) {
                    var jsTypeName = (string)oDict["_"];
                    type = this.typeMap.GetTypeByName(jsTypeName);
                }
                if (type.IsValueType) {
                    return decode(oDict["v"], type);
                }
                if (type.IsArray) {
                    var elementType = type.GetElementType();
                    var elements = ((object[])oDict["v"]).Select(x => decode(x, elementType)).ToArray();
                    var ret = (Array)Activator.CreateInstance(type, elements.Length);
                    for (int i = 0; i < elements.Length; i++) {
                        var item = elements[i];
                        if (item is DecodeRef) {
                            var refId = ((DecodeRef)item).id;
                            var iCopy = i;
                            refs.Add(() => ret.SetValue(objs[refId], iCopy));
                        } else {
                            ret.SetValue(item, i);
                        }
                    }
                    return ret;
                } else {
                    foreach (var customTypeCodec in customTypeCodecs) {
                        if (customTypeCodec.IsType(type)) {
                            var custom = customTypeCodec.Decode(o, type, this.typeMap, fn => refs.Add(() => fn(objs)), decode);
                            return custom;
                        }
                    }
                    var ret = Activator.CreateInstance(type, true);
                    foreach (var field in oDict.Where(x => x.Key != "_")) {
                        var fieldInfo = this.typeMap.GetFieldByName(type, field.Key);
                        var fieldValue = decode(field.Value, fieldInfo.FieldType);
                        if (fieldValue is DecodeRef) {
                            var refId = ((DecodeRef)fieldValue).id;
                            refs.Add(() => fieldInfo.SetValue(ret, objs[refId]));
                        } else {
                            fieldInfo.SetValue(ret, fieldValue);
                        }
                    }
                    return ret;
                }
            };
            foreach (var objJson in json) {
                var objId = (string)objJson[0];
                var obj = decode(objJson[1], null);
                objs.Add(objId, obj);
            }

            // List can expand, so cannot be done in a foreach loop
            for (int i = 0; i < refs.Count; i++) {
                refs[i]();
            }

            var obj0 = (T)objs["0"];
            return obj0;
        }

    }
}
