using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SolidBoxGE.IO;

namespace SolidBoxGE.Core.Render.OpenGL
{
    internal class OpenGLRenderer : GERenderer
    {
        private GL _gl;

        private uint _spriteRenderVao;
        private uint _spriteRenderVbo;

        private GLTexture _tex;
        private ShaderProgram _program;

        public OpenGLRenderer(GL gl)
        {
            _gl = gl;
        }

        public override unsafe void Init()
        {
            _gl.ClearColor(.1f, .1f, .1f, 1.0f);

            _spriteRenderVao = _gl.GenVertexArray();

            _gl.BindVertexArray(_spriteRenderVao);

            _spriteRenderVbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _spriteRenderVbo);

            float[] vertices = new float[4 * 3];
            fixed (float* bufferPtr = &vertices[0])
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)vertices.Length * sizeof(float), bufferPtr, BufferUsageARB.StaticDraw);

            _gl.BindVertexArray(0);

            _tex = GLTexture.LoadTexture2D(_gl, "./Data/Sprites/alph.png", GLTextureParameters.Pixelated);

            _program = new ShaderProgram(_gl, ShaderLoader.LoadShader("./Data/Shaders/sprite.vert"), ShaderLoader.LoadShader("./Data/Shaders/sprite.frag"));
        }

        public override unsafe void Render(double deltaTime)
        {
            _gl.Viewport(0, 0, (uint)ScreenSize.X, (uint)ScreenSize.Y);

            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _program.UseProgram();
            _program.SetMatrix4(_program.GetLocation("local"), Matrix4X4<float>.Identity);
            _program.SetMatrix4(_program.GetLocation("world"), Matrix4X4<float>.Identity);
            _program.SetMatrix4(_program.GetLocation("view"), Matrix4X4<float>.Identity);

            _gl.BindTexture(TextureTarget.Texture2D, _tex.Handle);

            _gl.BindVertexArray(_spriteRenderVao);
            _gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}
