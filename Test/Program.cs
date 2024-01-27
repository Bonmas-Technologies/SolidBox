using Silk.NET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;
using System.Text;

namespace Test
{
    internal class Program
    {
        private static GL _gl;

        private static Task _writeTask;
        private static IWindow _main;
        private static IInputContext _inputContext;

        private static TextWriter _output;
        private static StringBuilder _counter;

        private static uint _program;

        private static uint _vao;
        private static uint _vbo;
        private static uint _ebo;

        private static ulong _frameCounter;

        private static float[] vertices =
        {
            -0.5f,  0.5f, 0,
             0.5f,  0.5f, 0,
            -0.5f, -0.5f, 0,
             0.5f, -0.5f, 0,
        };

        private static uint[] elements =
        {
            1, 0, 2,
            2, 3, 1
        };

        private const string vertexCode =
@"
#version 330 core

layout (location = 0) in vec3 aPosition;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
}
";
        private const string fragmentCode =
@"
#version 330 core

out vec4 out_color;

void main()
{
    out_color = vec4(1., .5, .2, 1.);
}
";

        static void Main(string[] args)
        {
            _output = Console.Out;
            _counter = new StringBuilder();

            _main = Window.Create(WindowOptions.Default with
            {
                Size = new Vector2D<int>(800, 600),
                Title = "Hello world!",
                VSync = true,
            });

            _main.Load += OnLoad;
            _main.Update += Update;
            _main.Render += Render;
            _main.Resize += Resize;

            _main.Run();
        }


        static unsafe void OnLoad()
        {
            _gl = _main.CreateOpenGL();

            _inputContext = _main.CreateInput();

            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
                _inputContext.Keyboards[i].KeyDown += KeyDown;

            _gl.ClearColor(0.3906f, 0.5802f, 0.9257f, 1);

            unsafe
            {
                int major = 0;
                int minor = 0;

                _gl.GetInteger(GetPName.MajorVersion, &major);
                _gl.GetInteger(GetPName.MajorVersion, &minor);

                _main.Title = string.Format("{0}: {1}.{2}", _main.Title, major, minor);

                _vao = _gl.GenVertexArray();

                _gl.BindVertexArray(_vao);

                _vbo = _gl.GenBuffer();
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                fixed (float* buffer = vertices)
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) vertices.Length * sizeof(float), buffer, BufferUsageARB.StaticDraw);

                _ebo = _gl.GenBuffer();
                _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
                fixed (uint* buffer = elements)
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) elements.Length * sizeof(uint), buffer, BufferUsageARB.StaticDraw);

                var vert = _gl.CreateShader(ShaderType.VertexShader);

                _gl.ShaderSource(vert, vertexCode);
                _gl.CompileShader(vert);

                int vertStatus = 0;
                _gl.GetShader(vert, ShaderParameterName.CompileStatus, &vertStatus);

                if (vertStatus == 0)
                    throw new Exception("vertex compile error");


                var frag = _gl.CreateShader(ShaderType.FragmentShader);

                _gl.ShaderSource(frag, fragmentCode);
                _gl.CompileShader(frag);

                int fragStatus = 0;
                _gl.GetShader(frag, ShaderParameterName.CompileStatus, &fragStatus);

                if (fragStatus == 0)
                    throw new Exception("fragment compile error");

                _program = _gl.CreateProgram();

                _gl.AttachShader(_program, vert);
                _gl.AttachShader(_program, frag);
                _gl.LinkProgram(_program);

                int linkStatus = 0;

                _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, &linkStatus);

                if (linkStatus == 0)
                    throw new Exception("fragment compile error");

                _gl.DetachShader(_program, vert);
                _gl.DetachShader(_program, frag);
                _gl.DeleteShader(vert);
                _gl.DeleteShader(frag);

                _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
                _gl.EnableVertexAttribArray(0);

                _gl.BindVertexArray(0);
            }

            _gl.Enable(EnableCap.CullFace);
            _gl.CullFace(TriangleFace.Back);
            _gl.FrontFace(FrontFaceDirection.Ccw);

        }


        static void Update(double time)
        {
            _frameCounter++;

            if (_writeTask == null || _writeTask.IsCompleted)
            {
                _counter.Clear();
                _counter.AppendFormat("fps: {0}, frame: {1}{2}", Math.Round(1 / time), _frameCounter, Environment.NewLine);
                _writeTask = _output.WriteAsync(_counter);
            }
        }

        static unsafe void Render(double time)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _gl.UseProgram(_program);
            _gl.BindVertexArray(_vao);
            _gl.DrawElements(PrimitiveType.Triangles, (uint) elements.Length, DrawElementsType.UnsignedInt, (void*) 0);
        }

        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _main.Close();
        }

        private static void Resize(Vector2D<int> size)
        {
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y); // TODO: во время отложенного освещения и теней, этот код нужно переделать
        }
    }
}