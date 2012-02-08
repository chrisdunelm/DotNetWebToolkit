using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("ArrayBuffer")]
    public class ArrayBuffer {
    }

    [JsAbstractClass]
    public abstract class ArrayBufferView {
        public extern ArrayBuffer Buffer { get; }
        public extern uint ByteOffset { get; }
        public extern uint ByteLength { get; }
    }

    [JsClass("Float32Array")]
    public class Float32Array : ArrayBufferView {

        public extern Float32Array(float[] array);

    }

}
