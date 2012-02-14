using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Web;
using NUnit.Framework;

namespace Test.BrowserTests.TypedArrays {

    [TestFixture]
    public class TestDataView : BrowserTestBase {

        [Test]
        public void TestCtorArrayBuffer() {
            Action f = () => {
                var a = new UInt8Array(new byte[] { 1, 2, 3, 4 });
                var v = new DataView(a.Buffer);
                Done(v.GetUInt16(0) == 0x0102);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorArrayBufferOffset() {
            Action f = () => {
                var a = new UInt8Array(new byte[] { 1, 2, 3, 4 });
                var v = new DataView(a.Buffer, 2);
                Done(v.ByteLength == 2 && v.GetUInt16(0) == 0x0304);
            };
            this.Start(f);
        }

        [Test]
        public void TestCtorArrayBufferOffsetLength() {
            Action f = () => {
                var a = new UInt8Array(new byte[] { 1, 2, 3, 4 });
                var v = new DataView(a.Buffer, 1, 1);
                Done(v.ByteLength == 1 && v.GetUInt8(0) == 2);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetInt8() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetInt8(0, 5);
                v.SetInt8(8, 6);
                Done(v.GetInt8(0) == 5 && v.GetInt8(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetUInt8() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetUInt8(0, 5);
                v.SetUInt8(8, 6);
                Done(v.GetUInt8(0) == 5 && v.GetUInt8(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetInt16() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetInt16(0, 5);
                v.SetInt16(8, 6);
                Done(v.GetInt16(0) == 5 && v.GetInt16(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetUInt16() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetUInt16(0, 5);
                v.SetUInt16(8, 6);
                Done(v.GetUInt16(0) == 5 && v.GetUInt16(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetInt32() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetInt32(0, 5);
                v.SetInt32(8, 6);
                Done(v.GetInt32(0) == 5 && v.GetInt32(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetUInt32() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetUInt32(0, 5);
                v.SetUInt32(8, 6);
                Done(v.GetUInt32(0) == 5 && v.GetUInt32(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetFloat32() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetFloat32(0, 5);
                v.SetFloat32(8, 6);
                Done(v.GetFloat32(0) == 5 && v.GetFloat32(8) == 6);
            };
            this.Start(f);
        }

        [Test]
        public void TestSetGetFloat64() {
            Action f = () => {
                var b = new ArrayBuffer(16);
                var v = new DataView(b);
                v.SetFloat64(0, 5);
                v.SetFloat64(8, 6);
                Done(v.GetFloat64(0) == 5 && v.GetFloat64(8) == 6);
            };
            this.Start(f);
        }

    }

}
