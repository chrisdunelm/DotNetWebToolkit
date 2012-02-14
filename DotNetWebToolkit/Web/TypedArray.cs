using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("ArrayBuffer")]
    public sealed class ArrayBuffer {
        public extern ArrayBuffer(uint length);
        public extern uint ByteLength { get; }
    }

    [JsAbstractClass]
    public abstract class ArrayBufferView {
        public extern ArrayBuffer Buffer { get; }
        public extern uint ByteOffset { get; }
        public extern uint ByteLength { get; }
    }

    [JsClass("Int8Array")]
    public sealed class Int8Array : ArrayBufferView {
        public const uint BytesPerElement = 1;
        public extern Int8Array(uint size);
        public extern Int8Array(SByte[] array);
        public extern Int8Array(Int8Array array);
        public extern Int8Array(ArrayBuffer buffer);
        public extern Int8Array(ArrayBuffer buffer, uint byteOffset);
        public extern Int8Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern SByte this[int index] { get; set; }
        public extern void Set(Int8Array array);
        public extern void Set(Int8Array array, uint offset);
        public extern void Set(SByte[] array);
        public extern void Set(SByte[] array, uint offset);
        public extern Int8Array Subarray(int begin);
        public extern Int8Array Subarray(int begin, int end);
    }

    [JsClass("Uint8Array")]
    public class UInt8Array : ArrayBufferView {
        public const uint BytesPerElement = 1;
        public extern UInt8Array(uint size);
        public extern UInt8Array(Byte[] array);
        public extern UInt8Array(UInt8Array array);
        public extern UInt8Array(ArrayBuffer buffer);
        public extern UInt8Array(ArrayBuffer buffer, uint byteOffset);
        public extern UInt8Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern Byte this[int index] { get; set; }
        public extern void Set(UInt8Array array);
        public extern void Set(UInt8Array array, uint offset);
        public extern void Set(Byte[] array);
        public extern void Set(Byte[] array, uint offset);
        public extern UInt8Array Subarray(int begin);
        public extern UInt8Array Subarray(int begin, int end);
    }

    [JsClass("Uint8ClampedArray")]
    public sealed class UInt8ClampedArray : UInt8Array {
        public extern UInt8ClampedArray(uint size);
        public extern UInt8ClampedArray(Byte[] array);
        public extern UInt8ClampedArray(UInt8Array array);
        public extern UInt8ClampedArray(UInt8ClampedArray array);
        public extern UInt8ClampedArray(ArrayBuffer buffer);
        public extern UInt8ClampedArray(ArrayBuffer buffer, uint byteOffset);
        public extern UInt8ClampedArray(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern void Set(UInt8ClampedArray array);
        public extern void Set(UInt8ClampedArray array, uint offset);
        public extern new UInt8ClampedArray Subarray(int begin);
        public extern new UInt8ClampedArray Subarray(int begin, int end);
    }

    [JsClass("Int16Array")]
    public sealed class Int16Array : ArrayBufferView {
        public const uint BytesPerElement = 2;
        public extern Int16Array(uint size);
        public extern Int16Array(Int16[] array);
        public extern Int16Array(Int16Array array);
        public extern Int16Array(ArrayBuffer buffer);
        public extern Int16Array(ArrayBuffer buffer, uint byteOffset);
        public extern Int16Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern Int16 this[int index] { get; set; }
        public extern void Set(Int16Array array);
        public extern void Set(Int16Array array, uint offset);
        public extern void Set(Int16[] array);
        public extern void Set(Int16[] array, uint offset);
        public extern Int16Array Subarray(int begin);
        public extern Int16Array Subarray(int begin, int end);
    }

    [JsClass("Uint16Array")]
    public sealed class UInt16Array : ArrayBufferView {
        public const uint BytesPerElement = 2;
        public extern UInt16Array(uint size);
        public extern UInt16Array(UInt16[] array);
        public extern UInt16Array(UInt16Array array);
        public extern UInt16Array(ArrayBuffer buffer);
        public extern UInt16Array(ArrayBuffer buffer, uint byteOffset);
        public extern UInt16Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern UInt16 this[int index] { get; set; }
        public extern void Set(UInt16Array array);
        public extern void Set(UInt16Array array, uint offset);
        public extern void Set(UInt16[] array);
        public extern void Set(UInt16[] array, uint offset);
        public extern UInt16Array Subarray(int begin);
        public extern UInt16Array Subarray(int begin, int end);
    }

    [JsClass("Int32Array")]
    public sealed class Int32Array : ArrayBufferView {
        public const uint BytesPerElement = 4;
        public extern Int32Array(uint size);
        public extern Int32Array(Int32[] array);
        public extern Int32Array(Int32Array array);
        public extern Int32Array(ArrayBuffer buffer);
        public extern Int32Array(ArrayBuffer buffer, uint byteOffset);
        public extern Int32Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern Int32 this[int index] { get; set; }
        public extern void Set(Int32Array array);
        public extern void Set(Int32Array array, uint offset);
        public extern void Set(Int32[] array);
        public extern void Set(Int32[] array, uint offset);
        public extern Int32Array Subarray(int begin);
        public extern Int32Array Subarray(int begin, int end);
    }

    [JsClass("Uint32Array")]
    public sealed class UInt32Array : ArrayBufferView {
        public const uint BytesPerElement = 4;
        public extern UInt32Array(uint size);
        public extern UInt32Array(UInt32[] array);
        public extern UInt32Array(UInt32Array array);
        public extern UInt32Array(ArrayBuffer buffer);
        public extern UInt32Array(ArrayBuffer buffer, uint byteOffset);
        public extern UInt32Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern UInt32 this[int index] { get; set; }
        public extern void Set(UInt32Array array);
        public extern void Set(UInt32Array array, uint offset);
        public extern void Set(UInt32[] array);
        public extern void Set(UInt32[] array, uint offset);
        public extern UInt32Array Subarray(int begin);
        public extern UInt32Array Subarray(int begin, int end);
    }

    [JsClass("Float32Array")]
    public sealed class Float32Array : ArrayBufferView {
        public const uint BytesPerElement = 4;
        public extern Float32Array(uint size);
        public extern Float32Array(float[] array);
        public extern Float32Array(Float32Array array);
        public extern Float32Array(ArrayBuffer buffer);
        public extern Float32Array(ArrayBuffer buffer, uint byteOffset);
        public extern Float32Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern float this[int index] { get; set; }
        public extern void Set(Float32Array array);
        public extern void Set(Float32Array array, uint offset);
        public extern void Set(float[] array);
        public extern void Set(float[] array, uint offset);
        public extern Float32Array Subarray(int begin);
        public extern Float32Array Subarray(int begin, int end);
    }

    [JsClass("Float64Array")]
    public sealed class Float64Array : ArrayBufferView {
        public const uint BytesPerElement = 8;
        public extern Float64Array(uint size);
        public extern Float64Array(double[] array);
        public extern Float64Array(Float64Array array);
        public extern Float64Array(ArrayBuffer buffer);
        public extern Float64Array(ArrayBuffer buffer, uint byteOffset);
        public extern Float64Array(ArrayBuffer buffer, uint byteOffset, uint length);
        public extern uint Length { get; }
        public extern double this[int index] { get; set; }
        public extern void Set(Float64Array array);
        public extern void Set(Float64Array array, uint offset);
        public extern void Set(double[] array);
        public extern void Set(double[] array, uint offset);
        public extern Float64Array Subarray(int begin);
        public extern Float64Array Subarray(int begin, int end);
    }

    [JsClass("DataView")]
    public sealed class DataView : ArrayBufferView {
        public extern DataView(ArrayBuffer buffer);
        public extern DataView(ArrayBuffer buffer, uint byteOffset);
        public extern DataView(ArrayBuffer buffer, uint byteOffset, uint byteLength);
        public extern SByte GetInt8(uint byteOffset);
        [JsDetail(Name = "getUint8")]
        public extern Byte GetUInt8(uint byteOffset);
        public extern Int16 GetInt16(uint byteOffset, bool littleEndian = false);
        [JsDetail(Name = "getUint16")]
        public extern UInt16 GetUInt16(uint byteOffset, bool littleEndian = false);
        public extern Int32 GetInt32(uint byteOffset, bool littleEndian = false);
        [JsDetail(Name = "getUint32")]
        public extern UInt32 GetUInt32(uint byteOffset, bool littleEndian = false);
        public extern Single GetFloat32(uint byteOffset, bool littleEndian = false);
        public extern Double GetFloat64(uint byteOffset, bool littleEndian = false);
        public extern void SetInt8(uint byteOffset, SByte value);
        [JsDetail(Name = "setUint8")]
        public extern void SetUInt8(uint byteOffset, Byte value);
        public extern void SetInt16(uint byteOffset, Int16 value, bool littleEndian = false);
        [JsDetail(Name = "setUint16")]
        public extern void SetUInt16(uint byteOffset, UInt16 value, bool littleEndian = false);
        public extern void SetInt32(uint byteOffset, Int32 value, bool littleEndian = false);
        [JsDetail(Name = "setUint32")]
        public extern void SetUInt32(uint byteOffset, UInt32 value, bool littleEndian = false);
        public extern void SetFloat32(uint byteOffset, Single value, bool littleEndian = false);
        public extern void SetFloat64(uint byteOffset, Double value, bool littleEndian = false);
    }

}
