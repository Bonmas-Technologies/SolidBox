using System;
using Silk.NET.OpenGL;
using System.Runtime.Serialization;
using Silk.NET.Maths;

namespace SolidBoxGE.Core.Render.OpenGL
{
    internal class ShaderProgram : IDisposable
    {
        private readonly GL _context;
        private readonly uint _program;

        private string _errorLogBuffer;

        public ShaderProgram(GL context, string vertexCode, string fragmentCode)
        {
            _context = context;

            if (!LoadShader(ShaderType.VertexShader, vertexCode, out uint vert))
                throw new VertexShaderException(_errorLogBuffer);

            if (!LoadShader(ShaderType.FragmentShader, fragmentCode, out uint frag))
                throw new FragmentShaderException(_errorLogBuffer);

            if (!CreateProgram(vert, frag, out uint program))
                throw new ProgramLinkException(_errorLogBuffer);

            DeleteShader(program, vert);
            DeleteShader(program, frag);

            _program = program;
        }


        public void UseProgram()
        {
            _context.UseProgram(_program);
        }

        #region Uniforms

        public int GetLocation(string name) => _context.GetUniformLocation(_program, name);

        public void SetInt(int index, int value) => _context.Uniform1(index, value);

        public void SetFloat1(int index, float value) => _context.Uniform1(index, value);

        public void SetFloat2(int index, Vector2D<float> value) => _context.Uniform2(index, value.X, value.Y);

        public void SetFloat3(int index, Vector3D<float> value) => _context.Uniform3(index, value.X, value.Y, value.Z);

        public void SetFloat4(int index, Vector4D<float> value) => _context.Uniform4(index, value.X, value.Y, value.Z, value.W);

        public unsafe void SetMatrix4(int index, Matrix4X4<float> value) => _context.UniformMatrix4(index, 1, false, (float*)&value);

        #endregion

        public void Dispose()
        {
            _context.DeleteProgram(_program);
        }

        private unsafe bool LoadShader(ShaderType type, string code, out uint shader)
        {
            shader = _context.CreateShader(type);

            _context.ShaderSource(shader, code);
            _context.CompileShader(shader);

            int status;
            _context.GetShader(shader, ShaderParameterName.CompileStatus, &status);

            if (status == 0)
            {
                _errorLogBuffer = _context.GetShaderInfoLog(shader);

                _context.DeleteShader(shader);
                shader = 0;
                return false;
            }

            return true;
        }

        private void DeleteShader(uint program, uint shader)
        {
            _context.DetachShader(program, shader);
            _context.DeleteShader(shader);
        }

        private unsafe bool CreateProgram(uint vert, uint frag, out uint program)
        {
            program = _context.CreateProgram();

            _context.AttachShader(program, vert);
            _context.AttachShader(program, frag);
            _context.LinkProgram(program);

            int status = 0;

            _context.GetProgram(program, ProgramPropertyARB.LinkStatus, &status);

            if (status == 0)
            {
                _errorLogBuffer = _context.GetProgramInfoLog(program);
                _context.DeleteProgram(program);
                program = 0;
                return false;
            }

            return true;
        }
    }

    [Serializable]
    public class ProgramLinkException : Exception
    {
        public ProgramLinkException() { }
        public ProgramLinkException(string message) : base(message) { }
        public ProgramLinkException(string message, Exception inner) : base(message, inner) { }
        protected ProgramLinkException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class VertexShaderException : Exception
    {
        public VertexShaderException() { }
        public VertexShaderException(string message) : base(message) { }
        public VertexShaderException(string message, Exception innerException) : base(message, innerException) { }
        protected VertexShaderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FragmentShaderException : Exception
    {
        public FragmentShaderException() { }
        public FragmentShaderException(string message) : base(message) { }
        public FragmentShaderException(string message, Exception inner) : base(message, inner) { }
        protected FragmentShaderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
