using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    using GLenum = UInt32;

    [JsClass("WebGLRenderingContext")]
    public class WebGLRenderingContext : CanvasRenderingContext {

        // Buffer Objects
        public const GLenum ARRAY_BUFFER = 0x8892;
        public const GLenum ELEMENT_ARRAY_BUFFER = 0x8893;
        public const GLenum ARRAY_BUFFER_BINDING = 0x8894;
        public const GLenum ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;
        public const GLenum STREAM_DRAW = 0x88E0;
        public const GLenum STATIC_DRAW = 0x88E4;
        public const GLenum DYNAMIC_DRAW = 0x88E8;

        // EnableCap
        // TEXTURE_2D
        public const GLenum CULL_FACE = 0x0B44;
        public const GLenum BLEND = 0x0BE2;
        public const GLenum DITHER = 0x0BD0;
        public const GLenum STENCIL_TEST = 0x0B90;
        public const GLenum DEPTH_TEST = 0x0B71;
        public const GLenum SCISSOR_TEST = 0x0C11;
        public const GLenum POLYGON_OFFSET_FILL = 0x8037;
        public const GLenum SAMPLE_ALPHA_TO_COVERAGE = 0x809E;
        public const GLenum SAMPLE_COVERAGE = 0x80A0;
        public const GLenum DEPTH_BUFFER_BIT = 0x00000100;

        // Shaders
        public const GLenum FRAGMENT_SHADER = 0x8B30;
        public const GLenum VERTEX_SHADER = 0x8B31;
        public const GLenum MAX_VERTEX_ATTRIBS = 0x8869;
        public const GLenum MAX_VERTEX_UNIFORM_VECTORS = 0x8DFB;
        public const GLenum MAX_VARYING_VECTORS = 0x8DFC;
        public const GLenum MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8B4D;
        public const GLenum MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C;
        public const GLenum MAX_TEXTURE_IMAGE_UNITS = 0x8872;
        public const GLenum MAX_FRAGMENT_UNIFORM_VECTORS = 0x8DFD;
        public const GLenum SHADER_TYPE = 0x8B4F;
        public const GLenum DELETE_STATUS = 0x8B80;
        public const GLenum LINK_STATUS = 0x8B82;
        public const GLenum VALIDATE_STATUS = 0x8B83;
        public const GLenum ATTACHED_SHADERS = 0x8B85;
        public const GLenum ACTIVE_UNIFORMS = 0x8B86;
        public const GLenum ACTIVE_ATTRIBUTES = 0x8B89;
        public const GLenum SHADING_LANGUAGE_VERSION = 0x8B8C;
        public const GLenum CURRENT_PROGRAM = 0x8B8D;

        public extern HtmlCanvasElement Canvas { get; }
        public extern void BindBuffer(GLenum target, WebGLBuffer buffer);
        public extern void BufferData(GLenum target, ArrayBufferView data, GLenum usage);
        public extern void ClearColor(float red, float green, float blue, float alpha);
        public extern void CompileShader(WebGLShader shader);
        public extern WebGLShader CreateShader(GLenum type);
        public extern WebGLBuffer CreateBuffer();
        public extern void Enable(GLenum cap);
        public extern void ShaderSource(WebGLShader shader, string source);
        public extern void Viewport(int x, int y, int width, int height);

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
        public extern GLenum Type { get; }
        public extern string Name { get; }
    }

}
