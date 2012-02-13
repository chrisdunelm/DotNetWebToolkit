using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;
using DotNetWebToolkit.Web;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.WebGL {

    [JsClass("WebGLRenderingContext")]
    public class WebGLRenderingContext : CanvasRenderingContext {

        public extern HtmlCanvasElement Canvas { get; }

        public extern void ActiveTexture(TextureUnit texture);
        public extern void AttachShader(WebGLProgram program, WebGLShader shader);
        public extern void BindBuffer(BufferTarget target, WebGLBuffer buffer);
        public extern void BindTexture(TextureTarget target, WebGLTexture texture);
        public extern void BufferData(BufferTarget target, ArrayBufferView data, BufferUsage usage);
        public extern void Clear(ClearBufferMask mask);
        public extern void ClearColor(float red, float green, float blue, float alpha);
        public extern void CompileShader(WebGLShader shader);
        public extern WebGLBuffer CreateBuffer();
        public extern WebGLProgram CreateProgram();
        public extern WebGLShader CreateShader(ShaderType type);
        public extern WebGLTexture CreateTexture();
        public extern void DrawArrays(BeginMode mode, int first, int count);
        public extern void DrawElements(BeginMode mode, int size, DataType type, int offset);
        public extern void Enable(EnableCap cap);
        public extern void EnableVertexAttribArray(uint index);
        public extern uint GetAttribLocation(WebGLProgram program, string name);
        public extern object GetProgramParameter(WebGLProgram program, ProgramParameter pname);
        public extern string GetShaderInfoLog(WebGLShader shader);
        public extern object GetShaderParameter(WebGLShader shader, ShaderParameter pname);
        public extern WebGLUniformLocation GetUniformLocation(WebGLProgram program, string name);
        public extern void LinkProgram(WebGLProgram program);
        public extern void PixelStorei(PixelStoreParameter pname, int param);
        public extern void ShaderSource(WebGLShader shader, string source);
        public extern void TexImage2D(TextureTarget target, int level, PixelFormat internalFormat, PixelFormat format, DataType type, HtmlImageElement image);
        public extern void TexImage2D(TextureTarget target, int level, PixelFormat internalFormat, PixelFormat format, DataType type, HtmlCanvasElement image);
        public extern void TexImage2D(TextureTarget target, int level, PixelFormat internalFormat, PixelFormat format, DataType type, HtmlVideoElement image);
        public extern void TexParameteri(TextureTarget target, TextureParameterName pname, int param);
        public extern void TexParameteri(TextureTarget target, TextureParameterName pname, TextureMagFilter param);
        public extern void TexParameteri(TextureTarget target, TextureParameterName pname, TextureMinFilter param);
        public extern void TexParameteri(TextureTarget target, TextureParameterName pname, TextureWrapMode param);
        public extern void Uniform1i(WebGLUniformLocation location, int x);
        public extern void UniformMatrix4fv(WebGLUniformLocation location, bool transpose, Float32Array v);
        public extern void UniformMatrix4fv(WebGLUniformLocation location, bool transpose, float[] v);
        public extern void UseProgram(WebGLProgram program);
        public extern void Viewport(int x, int y, int width, int height);
        public extern void VertexAttribPointer(uint index, int size, DataType type, bool normalized, int stride, int offset);

    }

    [JsClass("WebGLContextAttributes")]
    public class WebGLContextAttributes {

        public extern WebGLContextAttributes();

        public extern bool Alpha { get; set; }
        public extern bool Depth { get; set; }
        public extern bool Stencil { get; set; }
        public extern bool Antialias { get; set; }
        public extern bool PremultipliedAlpha { get; set; }
        public extern bool PreserveDrawingBuffer { get; set; }

    }

    [JsAbstractClass]
    public abstract class WebGLObject {
    }

    [JsClass("WebGLBuffer")]
    public class WebGLBuffer : WebGLObject {
    }

    [JsClass("WebGLFrameBuffer")]
    public class WebGLFrameBuffer : WebGLObject {
    }

    [JsClass("WebGLProgram")]
    public class WebGLProgram : WebGLObject {
    }

    [JsClass("WebGLRenderBuffer")]
    public class WebGLRenderBuffer : WebGLObject {
    }

    [JsClass("WebGLShader")]
    public class WebGLShader : WebGLObject {
    }

    [JsClass("WebGLTexture")]
    public class WebGLTexture : WebGLObject {
    }

    [JsClass("WebGLUniformLocation")]
    public class WebGLUniformLocation {
    }

    [JsClass("WebGLActiveInfo")]
    public class WebGLActiveInfo {
        public extern int Size { get; }
        //public extern GLenum Type { get; }
        public extern string Name { get; }
    }

}
