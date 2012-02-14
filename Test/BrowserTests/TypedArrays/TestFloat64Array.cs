using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests.TypedArrays {

    using TypedArray = Float64Array;
    using ElementType = Double;

    [TestFixture]
    public class TestFloat64Array : BrowserTestBase {

        [Test]
        public void TestBytesPerElement() {
            Action f = () => {
                Done(TypedArray.BytesPerElement == 8);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorSize() {
            Action f = () => {
                var a = new TypedArray(2);
                Done(a.ByteLength == TypedArray.BytesPerElement * 2 && a.Length == 2 && a[0] == 0 && a[1] == 0);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorArray() {
            Action f = () => {
                var a = new TypedArray(new ElementType[] { 1, 2 });
                Done(a.ByteLength == TypedArray.BytesPerElement * 2 && a.Length == 2 && a[0] == 1 && a[1] == 2);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorTypedArray() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2 });
                var a = new TypedArray(b);
                Done(a.ByteLength == TypedArray.BytesPerElement * 2 && a.Length == 2 && a[0] == 1 && a[1] == 2);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorArrayBuffer() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2 });
                var a = new TypedArray(b.Buffer);
                Done(a.ByteLength == TypedArray.BytesPerElement * 2 && a.Length == 2 && a[0] == 1 && a[1] == 2);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorArrayBufferOffset() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2, 3 });
                var a = new TypedArray(b.Buffer, TypedArray.BytesPerElement);
                Done(a.ByteLength == TypedArray.BytesPerElement * 2 && a.Length == 2 && a[0] == 2 && a[1] == 3);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorArrayBufferOffsetLength() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2, 3 });
                var a = new TypedArray(b.Buffer, TypedArray.BytesPerElement, 1);
                Done(a.ByteLength == TypedArray.BytesPerElement * 1 && a.Length == 1 && a[0] == 2);
            };
            this.Start(f);
        }

        [Test]
        public void TestIndexer() {
            Action f = () => {
                var a = new TypedArray(1);
                a[0] = 99;
                Done(a[0] == 99);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetTypedArray() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2 });
                var a = new TypedArray(new ElementType[] { 6, 7, 8, 9 });
                a.Set(b);
                Done(a[0] == 1 && a[1] == 2 && a[2] == 8 && a[3] == 9);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetTypedArrayOffset() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2 });
                var a = new TypedArray(new ElementType[] { 6, 7, 8, 9 });
                a.Set(b, 1);
                Done(a[0] == 6 && a[1] == 1 && a[2] == 2 && a[3] == 9);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetArray() {
            Action f = () => {
                var a = new TypedArray(new ElementType[] { 6, 7, 8, 9 });
                a.Set(new ElementType[] { 1, 2 });
                Done(a[0] == 1 && a[1] == 2 && a[2] == 8 && a[3] == 9);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetArrayOffset() {
            Action f = () => {
                var a = new TypedArray(new ElementType[] { 6, 7, 8, 9 });
                a.Set(new ElementType[] { 1, 2 }, 1);
                Done(a[0] == 6 && a[1] == 1 && a[2] == 2 && a[3] == 9);
            };
            this.Start(f);
        }

        [Test]
        public void TestSubArrayBegin() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2, 3, 4 });
                var a = b.Subarray(1);
                Done(a.Length == 3 && a[0] == 2 && a[1] == 3 && a[2] == 4);
            };
            this.Start(f);
        }

        [Test]
        public void TestSubArrayBeginEnd() {
            Action f = () => {
                var b = new TypedArray(new ElementType[] { 1, 2, 3, 4 });
                var a = b.Subarray(1, 3);
                Done(a.Length == 2 && a[0] == 2 && a[1] == 3);
            };
            this.Start(f);
        }

    }

}
