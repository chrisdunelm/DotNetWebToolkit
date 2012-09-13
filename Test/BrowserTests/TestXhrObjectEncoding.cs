using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests {

    using Int8 = System.SByte;
    using UInt8 = System.Byte;

    [TestFixture]
    public class TestXhrObjectEncoding : BrowserTestBase {

        class XmlHttpRequest2<TSend, TRecv> {

            public XmlHttpRequest2(string url, Action<TRecv> onRecv) {
                this.url = url;
                this.onRecv = onRecv;
            }

            private string url;
            private Action<TRecv> onRecv;

            public void Send(TSend obj) {
                var xhr = new XMLHttpRequest();
                xhr.Open("POST", this.url);
                xhr.OnReadyStateChange = () => {
                    if (xhr.ReadyState == XMLHttpRequestReadyState.Done) {
                        var data = xhr.RecvJson<TRecv>();
                        this.onRecv(data);
                    }
                };
                xhr.SendJson(obj);
            }

        }

        private void JsSendTest<T>(Func<T, bool> checkFn, Action f) {
            bool hasPassed = false;
            this.SetUrlJson("/xhr", data => {
                var obj = this.GetJson().Decode<T>(data);
                hasPassed = checkFn(obj);
            });
            this.Start(f, null, wd => hasPassed);
        }
        private static void JsSendJs<T>(Func<T> genObj) {
            var xhr = new XmlHttpRequest2<T, object>("xhr", data => Pass());
            xhr.Send(genObj());
        }

        private void JsRecvTest<T>(Func<T> genObj, Action f) {
            this.SetUrlJson("/xhr", data => genObj());
            this.Start(f);
        }
        private static void JsRecvJs<T>(Func<T, bool> checkFn) {
            var xhr = new XmlHttpRequest2<object, T>("/xhr", data => Done(checkFn(data)));
            xhr.Send(null);
        }


        private static bool GenBoolTrue() { return true; }
        private static bool CheckBoolTrue(bool o) { return o == true; }
        [Test]
        public void TestJsSendBoolTrue() {
            JsSendTest<bool>(CheckBoolTrue, () => JsSendJs(GenBoolTrue));
        }
        [Test]
        public void TestJsRecvBoolTrue() {
            JsRecvTest(GenBoolTrue, () => JsRecvJs<bool>(CheckBoolTrue));
        }

        private static bool GenBoolFalse() { return false; }
        private static bool CheckBoolFalse(bool o) { return o == false; }
        [Test]
        public void TestJsSendBoolFalse() {
            JsSendTest<bool>(CheckBoolFalse, () => JsSendJs(GenBoolFalse));
        }
        [Test]
        public void TestJsRecvBoolFalse() {
            JsRecvTest(GenBoolFalse, () => JsRecvJs<bool>(CheckBoolFalse));
        }

        private static bool? GenBoolNullable() { return true; }
        private static bool CheckBoolNullable(bool? v) { return v == true; }
        [Test]
        public void TestJsSendBoolNullable() {
            JsSendTest<bool?>(CheckBoolNullable, () => JsSendJs(GenBoolNullable));
        }
        [Test]
        public void TestJsRecvBoolNullable() {
            JsRecvTest(GenBoolNullable, () => JsRecvJs<bool?>(CheckBoolNullable));
        }

        private static bool? GenBoolNullableNull() { return null; }
        private static bool CheckBoolNullableNull(bool? v) { return v == null; }
        [Test]
        public void TestJsSendBoolNullableNull() {
            JsSendTest<bool?>(CheckBoolNullableNull, () => JsSendJs(GenBoolNullableNull));
        }
        [Test]
        public void TestJsRecvBoolNullableNull() {
            JsRecvTest(GenBoolNullableNull, () => JsRecvJs<bool?>(CheckBoolNullableNull));
        }

        private static Int8 GenInt8() { return -9; }
        private static bool CheckInt8(Int8 v) { return v == -9; }
        [Test]
        public void TestJsSendInt8() {
            JsSendTest<Int8>(CheckInt8, () => JsSendJs(GenInt8));
        }
        [Test]
        public void TestJsRecvInt8() {
            JsRecvTest(GenInt8, () => JsRecvJs<Int8>(CheckInt8));
        }

        private static Int16 GenInt16() { return -9000; }
        private static bool CheckInt16(Int16 v) { return v == -9000; }
        [Test]
        public void TestJsSendInt16() {
            JsSendTest<Int16>(CheckInt16, () => JsSendJs(GenInt16));
        }
        [Test]
        public void TestJsRecvInt16() {
            JsRecvTest(GenInt16, () => JsRecvJs<Int16>(CheckInt16));
        }

        private static Int32 GenInt32() { return -4; }
        private static bool CheckInt32(Int32 o) { return o == -4; }
        [Test]
        public void TestJsSendInt32() {
            JsSendTest<Int32>(CheckInt32, () => JsSendJs(GenInt32));
        }
        [Test]
        public void TestJsRecvInt32() {
            JsRecvTest(GenInt32, () => JsRecvJs<Int32>(CheckInt32));
        }

        private static Int64 GenInt64() { return -2; }
        private static bool CheckInt64(Int64 o) { return o == -2; }
        [Test]
        public void TestJsSendInt64() {
            JsSendTest<Int64>(CheckInt64, () => JsSendJs(GenInt64));
        }
        [Test]
        public void TestJsRecvInt64() {
            JsRecvTest(GenInt64, () => JsRecvJs<Int64>(CheckInt64));
        }

        private static Int64? GenInt64Nullable() { return -2; }
        private static bool CheckInt64Nullable(Int64? v) { return v == -2; }
        [Test]
        public void TestJsSendInt64Nullable() {
            JsSendTest<Int64?>(CheckInt64Nullable, () => JsSendJs(GenInt64Nullable));
        }
        [Test]
        public void TestJsRecvInt64Nullable() {
            JsRecvTest(GenInt64Nullable, () => JsRecvJs<Int64?>(CheckInt64Nullable));
        }

        private static Int64? GenInt64NullableNull() { return null; }
        private static bool CheckInt64NullableNull(Int64? v) { return v == null; }
        [Test]
        public void TestJsSendInt64NullableNull() {
            JsSendTest<Int64?>(CheckInt64NullableNull, () => JsSendJs(GenInt64NullableNull));
        }
        [Test]
        public void TestJsRecvInt64NullableNull() {
            JsRecvTest(GenInt64NullableNull, () => JsRecvJs<Int64?>(CheckInt64NullableNull));
        }

        private static UInt8 GenUInt8() { return 255; }
        private static bool CheckUInt8(UInt8 v) { return v == 255; }
        [Test]
        public void TestJsSendUInt8() {
            JsSendTest<UInt8>(CheckUInt8, () => JsSendJs(GenUInt8));
        }
        [Test]
        public void TestJsRecvUInt8() {
            JsRecvTest(GenUInt8, () => JsRecvJs<UInt8>(CheckUInt8));
        }

        private static UInt16 GenUInt16() { return 65535; }
        private static bool CheckUInt16(UInt16 v) { return v == 65535; }
        [Test]
        public void TestJsSendUInt16() {
            JsSendTest<UInt16>(CheckUInt16, () => JsSendJs(GenUInt16));
        }
        [Test]
        public void TestJsRecvUInt16() {
            JsRecvTest(GenUInt16, () => JsRecvJs<UInt16>(CheckUInt16));
        }

        private static UInt32 GenUInt32() { return 0xffffffff; }
        private static bool CheckUInt32(UInt32 v) { return v == 0xffffffff; }
        [Test]
        public void TestJsSendUInt32() {
            JsSendTest<UInt32>(CheckUInt32, () => JsSendJs(GenUInt32));
        }
        [Test]
        public void TestJsRecvUInt32() {
            JsRecvTest(GenUInt32, () => JsRecvJs<UInt32>(CheckUInt32));
        }

        private static UInt64 GenUInt64() { return 0x1234567890UL; }
        private static bool CheckUInt64(UInt64 v) { return v == 0x1234567890UL; }
        [Test]
        public void TestJsSendUInt64() {
            JsSendTest<UInt64>(CheckUInt64, () => JsSendJs(GenUInt64));
        }
        [Test]
        public void TestJsRecvUInt64() {
            JsRecvTest(GenUInt64, () => JsRecvJs<UInt64>(CheckUInt64));
        }

        private static Single GenSingle() { return 1.1f; }
        private static bool CheckSingle(Single v) { return v == 1.1f; }
        [Test]
        public void TestJsSendSingle() {
            JsSendTest<Single>(CheckSingle, () => JsSendJs(GenSingle));
        }
        [Test]
        public void TestJsRecvSingle() {
            JsRecvTest(GenSingle, () => JsRecvJs<Single>(CheckSingle));
        }

        private static Single GenSingleNaN() { return Single.NaN; }
        private static bool CheckSingleNaN(Single v) { return Single.IsNaN(v); }
        [Test]
        public void TestJsSendSingleNaN() {
            JsSendTest<Single>(CheckSingleNaN, () => JsSendJs(GenSingleNaN));
        }
        [Test]
        public void TestJsRecvSingleNaN() {
            JsRecvTest(GenSingleNaN, () => JsRecvJs<Single>(CheckSingleNaN));
        }

        private static Single GenSinglePositiveInfinity() { return Single.PositiveInfinity; }
        private static bool CheckSinglePositiveInfinity(Single v) { return Single.IsPositiveInfinity(v); }
        [Test]
        public void TestJsSendSinglePositiveInfinity() {
            JsSendTest<Single>(CheckSinglePositiveInfinity, () => JsSendJs(GenSinglePositiveInfinity));
        }
        [Test]
        public void TestJsRecvSinglePositiveInfinity() {
            JsRecvTest(GenSinglePositiveInfinity, () => JsRecvJs<Single>(CheckSinglePositiveInfinity));
        }

        private static Single GenSingleNegativeInfinity() { return Single.NegativeInfinity; }
        private static bool CheckSingleNegativeInfinity(Single v) { return Single.IsNegativeInfinity(v); }
        [Test]
        public void TestJsSendSingleNegativeInfinity() {
            JsSendTest<Single>(CheckSingleNegativeInfinity, () => JsSendJs(GenSingleNegativeInfinity));
        }
        [Test]
        public void TestJsRecvSingleNegativeInfinity() {
            JsRecvTest(GenSingleNegativeInfinity, () => JsRecvJs<Single>(CheckSingleNegativeInfinity));
        }

        private static Double GenDouble() { return 1.1d; }
        private static bool CheckDouble(Double v) { return v == 1.1d; }
        [Test]
        public void TestJsSendDouble() {
            JsSendTest<Double>(CheckDouble, () => JsSendJs(GenDouble));
        }
        [Test]
        public void TestJsRecvDouble() {
            JsRecvTest(GenDouble, () => JsRecvJs<Double>(CheckDouble));
        }

        private static Double GenDoubleNaN() { return Double.NaN; }
        private static bool CheckDoubleNaN(Double v) { return Double.IsNaN(v); }
        [Test]
        public void TestJsSendDoubleNaN() {
            JsSendTest<Double>(CheckDoubleNaN, () => JsSendJs(GenDoubleNaN));
        }
        [Test]
        public void TestJsRecvDoubleNaN() {
            JsRecvTest(GenDoubleNaN, () => JsRecvJs<Double>(CheckDoubleNaN));
        }

        private static Double GenDoublePositiveInfinity() { return Double.PositiveInfinity; }
        private static bool CheckDoublePositiveInfinity(Double v) { return Double.IsPositiveInfinity(v); }
        [Test]
        public void TestJsSendDoublePositiveInfinity() {
            JsSendTest<Double>(CheckDoublePositiveInfinity, () => JsSendJs(GenDoublePositiveInfinity));
        }
        [Test]
        public void TestJsRecvDoublePositiveInfinity() {
            JsRecvTest(GenDoublePositiveInfinity, () => JsRecvJs<Double>(CheckDoublePositiveInfinity));
        }

        private static Double GenDoubleNegativeInfinity() { return Double.NegativeInfinity; }
        private static bool CheckDoubleNegativeInfinity(Double v) { return Double.IsNegativeInfinity(v); }
        [Test]
        public void TestJsSendDoubleNegativeInfinity() {
            JsSendTest<Double>(CheckDoubleNegativeInfinity, () => JsSendJs(GenDoubleNegativeInfinity));
        }
        [Test]
        public void TestJsRecvDoubleNegativeInfinity() {
            JsRecvTest(GenDoubleNegativeInfinity, () => JsRecvJs<Double>(CheckDoubleNegativeInfinity));
        }

        private static char GenChar() { return 'A'; }
        private static bool CheckChar(char v) { return v == 'A'; }
        [Test]
        public void TestJsSendChar() {
            JsSendTest<char>(CheckChar, () => JsSendJs(GenChar));
        }
        [Test]
        public void TestJsRecvChar() {
            JsRecvTest(GenChar, () => JsRecvJs<char>(CheckChar));
        }

        private static char GenChar0() { return '\0'; }
        private static bool CheckChar0(char v) { return v == '\0'; }
        [Test]
        public void TestJsSendChar0() {
            JsSendTest<char>(CheckChar0, () => JsSendJs(GenChar0));
        }
        [Test]
        public void TestJsRecvChar0() {
            JsRecvTest(GenChar0, () => JsRecvJs<char>(CheckChar0));
        }

        private static string GenString() { return "abc"; }
        private static bool CheckString(string v) { return v == "abc"; }
        [Test]
        public void TestJsSendString() {
            JsSendTest<string>(CheckString, () => JsSendJs(GenString));
        }
        [Test]
        public void TestJsRecvString() {
            JsRecvTest(GenString, () => JsRecvJs<string>(CheckString));
        }

        private static string GenStringNull() { return null; }
        private static bool CheckStringNull(string v) { return v == null; }
        [Test]
        public void TestJsSendStringNull() {
            JsSendTest<string>(CheckStringNull, () => JsSendJs(GenStringNull));
        }
        [Test]
        public void TestJsRecvStringNull() {
            JsRecvTest(GenStringNull, () => JsRecvJs<string>(CheckStringNull));
        }

        private static Int32[] GenInt32Array() { return new[] { -5, 0, 5, 999 }; }
        private static bool CheckInt32Array(Int32[] v) { return v.Length == 4 && v[0] == -5 && v[1] == 0 && v[2] == 5 && v[3] == 999; }
        [Test]
        public void TestJsSendInt32Array() {
            JsSendTest<Int32[]>(CheckInt32Array, () => JsSendJs(GenInt32Array));
        }
        [Test]
        public void TestJsRecvInt32Array() {
            JsRecvTest(GenInt32Array, () => JsRecvJs<Int32[]>(CheckInt32Array));
        }

        private static Int32?[] GenInt32NullableArray() { return new Int32?[] { -1, 0, null }; }
        private static bool CheckInt32NullableArray(Int32?[] v) { return v.Length == 3 && v[0] == -1 && v[1] == 0 && v[2] == null; }
        [Test]
        public void TestJsSendInt32NullableArray() {
            JsSendTest<Int32?[]>(CheckInt32NullableArray, () => JsSendJs(GenInt32NullableArray));
        }
        [Test]
        public void TestJsRecvInt32NullableArray() {
            JsRecvTest(GenInt32NullableArray, () => JsRecvJs<Int32?[]>(CheckInt32NullableArray));
        }

        private static Int64[] GenInt64Array() { return new[] { -5L, 0L, 5L, 0x100000002L }; }
        private static bool CheckInt64Array(Int64[] v) { return v.Length == 4 && v[0] == -5L && v[1] == 0L && v[2] == 5L && v[3] == 0x100000002L; }
        [Test]
        public void TestJsSendInt64Array() {
            JsSendTest<Int64[]>(CheckInt64Array, () => JsSendJs(GenInt64Array));
        }
        [Test]
        public void TestJsRecvInt64Array() {
            JsRecvTest(GenInt64Array, () => JsRecvJs<Int64[]>(CheckInt64Array));
        }

        private static Int64?[] GenInt64NullableArray() { return new Int64?[] { -2L, 0, null }; }
        private static bool CheckInt64NullableArray(Int64?[] v) { return v.Length == 3 && v[0] == -2L && v[1] == 0L && v[2] == null; }
        [Test]
        public void TestJsSendInt64NullableArray() {
            JsSendTest<Int64?[]>(CheckInt64NullableArray, () => JsSendJs(GenInt64NullableArray));
        }
        [Test]
        public void TestJsRecvInt64NullableArray() {
            JsRecvTest(GenInt64NullableArray, () => JsRecvJs<Int64?[]>(CheckInt64NullableArray));
        }

        class ObjectNoRefs {
            public int i;
            public long l;
            public string s;
            public char c;
            public bool b;
        }
        private static ObjectNoRefs GenObjectNoRefs() {
            return new ObjectNoRefs { i = -100, l = -200, s = "abc", c = 'a', b = true };
        }
        private static bool CheckObjectNoRefs(ObjectNoRefs v) {
            return v.i == -100 && v.l == -200 && v.s == "abc" && v.c == 'a' && v.b == true;
        }
        [Test]
        public void TestJsSendObjectNoRefs() {
            JsSendTest<ObjectNoRefs>(CheckObjectNoRefs, () => JsSendJs(GenObjectNoRefs));
        }
        [Test]
        public void TestJsRecvObjectNoRefs() {
            JsRecvTest(GenObjectNoRefs, () => JsRecvJs<ObjectNoRefs>(CheckObjectNoRefs));
        }

        class ObjectWithRefs {
            public class Inner {
                public int i;
            }
            public Inner i1, i2;
            public Inner iNull;
        }
        private static ObjectWithRefs GenObjectWithRefs() {
            return new ObjectWithRefs { i1 = new ObjectWithRefs.Inner { i = -1 }, i2 = new ObjectWithRefs.Inner { i = 1 }, iNull = null };
        }
        private static bool CheckObjectWithRefs(ObjectWithRefs v) {
            return v.i1.i == -1 && v.i2.i == 1 && v.iNull == null;
        }
        [Test]
        public void TestJsSendObjectWithRefs() {
            JsSendTest<ObjectWithRefs>(CheckObjectWithRefs, () => JsSendJs(GenObjectWithRefs));
        }
        [Test]
        public void TestJsRecvObjectWithRefs() {
            JsRecvTest(GenObjectWithRefs, () => JsRecvJs<ObjectWithRefs>(CheckObjectWithRefs));
        }

        class ObjectSelfRefs {
            public ObjectSelfRefs self1, self2, selfNull;
        }
        private static ObjectSelfRefs GenObjectSelfRefs() {
            var v = new ObjectSelfRefs();
            v.self1 = v.self2 = v;
            v.selfNull = null;
            return v;
        }
        private static bool CheckObjectSelfRefs(ObjectSelfRefs v) {
            return v.self1 == v && v.self2 == v && v.selfNull == null;
        }
        [Test]
        public void TestJsSendObjectSelfRefs() {
            JsSendTest<ObjectSelfRefs>(CheckObjectSelfRefs, () => JsSendJs(GenObjectSelfRefs));
        }
        [Test]
        public void TestJsRecvObjectSelfRefs() {
            JsRecvTest(GenObjectSelfRefs, () => JsRecvJs<ObjectSelfRefs>(CheckObjectSelfRefs));
        }

        class ObjectsMutualRefs1 {
            public ObjectsMutualRefs1 self;
            public ObjectsMutualRefs2 other;
        }
        class ObjectsMutualRefs2 {
            public ObjectsMutualRefs2 self;
            public ObjectsMutualRefs1 other;
        }
        private static ObjectsMutualRefs1 GenObjectsMutualRefs() {
            var v1 = new ObjectsMutualRefs1();
            var v2 = new ObjectsMutualRefs2();
            v1.self = v2.other = v1;
            v2.self = v1.other = v2;
            return v1;
        }
        private static bool CheckObjectsMutualRefs(ObjectsMutualRefs1 v) {
            return v.self == v && v.other.other == v && v.other.self == v.other && v.other.self == v.other.other.other;
        }
        [Test]
        public void TestJsSendObjectMutualRefs() {
            JsSendTest<ObjectsMutualRefs1>(CheckObjectsMutualRefs, () => JsSendJs(GenObjectsMutualRefs));
        }
        [Test]
        public void TestJsRecvObjectMutualRefs() {
            JsRecvTest(GenObjectsMutualRefs, () => JsRecvJs<ObjectsMutualRefs1>(CheckObjectsMutualRefs));
        }

        class NestedObjects {
            public class N1 {
                public class N2 {
                    public class N3 {
                        public string s;
                    }
                    public N3 n;
                }
                public N2 n;
            }
            public N1 n;
        }
        private static NestedObjects GenNestedObjects() {
            return new NestedObjects {
                n = new NestedObjects.N1 {
                    n = new NestedObjects.N1.N2 {
                        n = new NestedObjects.N1.N2.N3 {
                            s = "abc"
                        }
                    }
                }
            };
        }
        private static bool CheckNestedObjects(NestedObjects v) {
            return v.n.n.n.s == "abc";
        }
        [Test]
        public void TestJsSendNestedObjects() {
            JsSendTest<NestedObjects>(CheckNestedObjects, () => JsSendJs(GenNestedObjects));
        }
        [Test]
        public void TestJsRecvNestedObjects() {
            JsRecvTest(GenNestedObjects, () => JsRecvJs<NestedObjects>(CheckNestedObjects));
        }

        struct NestedStructs {
            public struct N1 {
                public struct N2 {
                    public struct N3 {
                        public string s;
                    }
                    public N3 n;
                }
                public N2 n;
            }
            public N1 n;
        }
        private static NestedStructs GenNestedStructs() {
            return new NestedStructs {
                n = new NestedStructs.N1 {
                    n = new NestedStructs.N1.N2 {
                        n = new NestedStructs.N1.N2.N3 {
                            s = "abc"
                        }
                    }
                }
            };
        }
        private static bool CheckNestedStructs(NestedStructs v) {
            return v.n.n.n.s == "abc";
        }
        [Test]
        public void TestJsSendNestedStructs() {
            JsSendTest<NestedStructs>(CheckNestedStructs, () => JsSendJs(GenNestedStructs));
        }
        [Test]
        public void TestJsRecvNestedStructs() {
            JsRecvTest(GenNestedStructs, () => JsRecvJs<NestedStructs>(CheckNestedStructs));
        }

        struct StructsInObject {
            public object a, b;
        }
        private static StructsInObject GenStructsInObject() {
            return new StructsInObject {
                a = new StructsInObject {
                    a = new StructsInObject {
                        a = 7L,
                        b = "abc"
                    },
                    b = 9d
                },
                b = new bool[] { true, false }
            };
        }
        private static bool CheckStructsInObject(StructsInObject v) {
            return
                (Int64)((StructsInObject)((StructsInObject)v.a).a).a == 7L &&
                (string)((StructsInObject)((StructsInObject)v.a).a).b == "abc" &&
                (Double)((StructsInObject)v.a).b == 9d &&
                ((bool[])v.b)[0] == true && ((bool[])v.b)[1] == false;
        }
        [Test]
        public void TestJsSendStructsInObject() {
            JsSendTest<StructsInObject>(CheckStructsInObject, () => JsSendJs(GenStructsInObject));
        }
        [Test]
        public void TestJsRecvStructsInObject() {
            JsRecvTest(GenStructsInObject, () => JsRecvJs<StructsInObject>(CheckStructsInObject));
        }

        class ArrayOfSelfRef {
            public ArrayOfSelfRef[] selfs1, selfs2;
        }
        private static ArrayOfSelfRef GenArrayOfSelfRef() {
            var o = new ArrayOfSelfRef();
            o.selfs1 = new[] { o, o, o, o };
            o.selfs2 = o.selfs1;
            return o;
        }
        private static bool CheckArrayOfSelfRef(ArrayOfSelfRef v) {
            if (v.selfs1.Length != 4 || v.selfs1 != v.selfs2) {
                return false;
            }
            for (int i = 0; i < v.selfs1.Length; i++) {
                if (v.selfs1[i] != v || v.selfs2[i] != v) {
                    return false;
                }
            }
            return true;
        }
        [Test]
        public void TestJsSendArrayOfSelfRef() {
            JsSendTest<ArrayOfSelfRef>(CheckArrayOfSelfRef, () => JsSendJs(GenArrayOfSelfRef));
        }
        [Test]
        public void TestJsRecvArrayOfSelfRef() {
            JsRecvTest(GenArrayOfSelfRef, () => JsRecvJs<ArrayOfSelfRef>(CheckArrayOfSelfRef));
        }

        class ArrayOfVariousClass {
            public int i;
        }
        struct ArrayOfVariousStruct {
            public long l;
        }
        private static object[] GenArrayOfVarious() {
            var a = new char[] { 'X', 'Y' };
            var b = new ArrayOfVariousClass { i = 99 };
            var c = new ArrayOfVariousStruct { l = -99 };
            return new object[] { null, a, b, c, a, b, c, "abc", 0xffffffffU, -2L };
        }
        private static bool SubCheckArrayOfVarious(object[] v) {
            if (v.Length != 10) {
                return false;
            }
            if (v[0] != null || (string)v[7] != "abc" || (UInt32)v[8] != 0xffffffffU || (Int64)v[9] != -2L) {
                return false;
            }
            if (v[1] != v[4] || v[2] != v[5] || v[3] == v[6]) {
                return false;
            }
            return true;
        }
        private static bool CheckArrayOfVarious(object[] v) {
            if (!SubCheckArrayOfVarious(v)) {
                return false;
            }
            var a1 = (char[])v[1];
            var a2 = (char[])v[4];
            if (a1[0] != 'X' || a1[1] != 'Y' || a2[0] != 'X' || a2[1] != 'Y') {
                return false;
            }
            if (((ArrayOfVariousClass)v[2]).i != 99 || ((ArrayOfVariousClass)v[5]).i != 99) {
                return false;
            }
            if (((ArrayOfVariousStruct)v[3]).l != -99 || ((ArrayOfVariousStruct)v[6]).l != -99) {
                return false;
            }
            return true;
        }
        [Test]
        public void TestJsSendArrayOfVarious() {
            JsSendTest<object[]>(CheckArrayOfVarious, () => JsSendJs(GenArrayOfVarious));
        }
        [Test]
        public void TestJsRecvArrayOfVarious() {
            JsRecvTest(GenArrayOfVarious, () => JsRecvJs<object[]>(CheckArrayOfVarious));
        }

        private static Int32[][] GenArrayThenEnumerate() {
            return new[]{
                new Int32[] { 3, 3, 3 }
            };
        }
        private static bool CheckArrayThenEnumerate(Int32[][] v) {
            return v.Length == 1 && v[0].Length == 3 && v[0].All(x => x == 3);
        }
        [Test]
        public void TestJsSendArrayThenEnumerate() {
            JsSendTest<Int32[][]>(CheckArrayThenEnumerate, () => JsSendJs(GenArrayThenEnumerate));
        }
        [Test]
        public void TestJsRecvArrayThenEnumerate() {
            JsRecvTest(GenArrayThenEnumerate, () => JsRecvJs<Int32[][]>(CheckArrayThenEnumerate));
        }

        class GenericObjects {
            public class Inner<T> {
                public T t;
            }
            public Inner<int> i;
            public Inner<string> s;
            public Inner<Inner<long>> ll;
            public Inner<Inner<Inner<char>>> ccc;
        }
        private static GenericObjects GenGenericObjects() {
            return new GenericObjects {
                i = new GenericObjects.Inner<int> { t = 44 },
                s = new GenericObjects.Inner<string> { t = "abc" },
                ll = new GenericObjects.Inner<GenericObjects.Inner<long>> {
                    t = new GenericObjects.Inner<long> { t = -44 },
                },
                ccc = new GenericObjects.Inner<GenericObjects.Inner<GenericObjects.Inner<char>>> {
                    t = new GenericObjects.Inner<GenericObjects.Inner<char>> {
                        t = new GenericObjects.Inner<char> { t = 'Z' },
                    },
                },
            };
        }
        private static bool CheckGenericObjects(GenericObjects v) {
            return v.i.t == 44 && v.s.t == "abc" && v.ll.t.t == -44 && v.ccc.t.t.t == 'Z';
        }
        [Test]
        public void TestJsSendGenericObjects() {
            JsSendTest<GenericObjects>(CheckGenericObjects, () => JsSendJs(GenGenericObjects));
        }
        [Test]
        public void TestJsRecvGenericObjects() {
            JsRecvTest(GenGenericObjects, () => JsRecvJs<GenericObjects>(CheckGenericObjects));
        }

        private static List<Int32> GenListInt32() {
            return new List<int> { 1, 2, 3, 4, 5 };
        }
        private static bool CheckListInt32(List<Int32> v) {
            return v.SequenceEqual(new[] { 1, 2, 3, 4, 5 });
        }
        [Test]
        public void TestJsSendListInt32() {
            JsSendTest<List<Int32>>(CheckListInt32, () => JsSendJs(GenListInt32));
        }
        [Test]
        public void TestJsRecvListInt32() {
            JsRecvTest(GenListInt32, () => JsRecvJs<List<Int32>>(CheckListInt32));
        }

        private static List<List<object>> GenListOfListsObjects() {
            return new List<List<object>> {
                null,
                new List<object>(),
                new List<object> { null, null },
                new List<object> { "abc", 1, true, -4L },
            };
        }
        private static bool CheckListOfListsObjects(List<List<object>> v) {
            return v.Count == 4 &&
                v[0] == null &&
                v[1].Count == 0 &&
                v[2].Count == 2 && v[2].All(x => x == null) &&
                v[3].Count == 4 &&
                (string)v[3][0] == "abc" &&
                (int)v[3][1] == 1 &&
                (bool)v[3][2] == true &&
                (long)v[3][3] == -4L;
        }
        [Test]
        public void TestJsSendListOfListsObjects() {
            JsSendTest<List<List<object>>>(CheckListOfListsObjects, () => JsSendJs(GenListOfListsObjects));
        }
        [Test]
        public void TestJsRecvListOfListsObects() {
            JsRecvTest(GenListOfListsObjects, () => JsRecvJs<List<List<object>>>(CheckListOfListsObjects));
        }

    }

}
