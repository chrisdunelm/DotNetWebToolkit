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
    }

    [JsAbstractClass]
    public abstract class ArrayBufferView {
        public extern ArrayBuffer Buffer { get; }
        public extern uint ByteOffset { get; }
        public extern uint ByteLength { get; }
    }

    [JsClass("Uint16Array")]
    public sealed class UInt16Array : ArrayBufferView {

        public extern UInt16Array(uint size);
        public extern UInt16Array(UInt16[] array);
        public extern UInt16Array(UInt16Array array);

        public extern UInt16 this[int index] { get; set; }

    }

    [JsClass("Float32Array")]
    public sealed class Float32Array : ArrayBufferView {

        public extern Float32Array(uint size);
        public extern Float32Array(float[] array);
        public extern Float32Array(Float32Array array);

        public extern float this[int index] { get; set; }

    }

}
