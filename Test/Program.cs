using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SolidBox.Engine.Core;
using System.Numerics;
using System.Text;

namespace Test
{
    internal class Program
    {
        private static GL _gl;

        private static IWindow _main;
        private static IInputContext _inputContext;
        private static ImGuiController _controller;

        private static TextWriter _output;
        private static StringBuilder _counter;

        private static ShaderProgram _program;
        private static ShaderProgram _buffer;
        private static Vector2D<int> _size;

        private static uint _scene;
        private static uint _screenVao;

        private static ulong _frameCounter;

        private static float[] frameVertices =
        {
            -1f,  1f, 0, 0, 1,
             1f,  1f, 0, 1, 1,
            -1f, -1f, 0, 0, 0, 
             1f, -1f, 0, 1, 0,
        };

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
        private static uint _screenBuffer;
        private static uint _texture;
        private static double _timer;
        private const string framebufferVertex =
@"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aUV;

out vec2 UV;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    UV = aUV;
}
";

        private const string frameBufferFragment =
@"
#version 330 core

uniform sampler2D buffer;

in vec2 UV;
out vec4 out_color;

void main()
{
    out_color = texture(buffer, UV);
}
";

        private const string screenVertex =
@"
#version 330 core

layout (location = 0) in vec3 aPosition;

uniform mat4 local;

void main()
{
    gl_Position = local * vec4(aPosition, 1.0);
}
";
        private const string screenFragment =
@"
#version 330 core

uniform vec3 color;

out vec4 out_color;

void main()
{
    out_color = vec4(color, 1.);
}
";

        private static float[] samples = new float[60];

        static void Main(string[] args)
        {
            _output = Console.Out;
            _counter = new StringBuilder();

            _size = new Vector2D<int>(800, 600);

            _main = Window.Create(WindowOptions.Default with
            {
                Size = _size,
                Title = "Hello world!",
                VSync = false,
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

            _controller = new ImGuiController(_gl, _main, _inputContext);

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

                _scene = _gl.GenVertexArray();

                _gl.BindVertexArray(_scene);
                {
                    var vbo = _gl.GenBuffer();
                    _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                    fixed (float* buffer = vertices)
                        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)vertices.Length * sizeof(float), buffer, BufferUsageARB.StaticDraw);

                    var ebo = _gl.GenBuffer();
                    _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
                    fixed (uint* buffer = elements)
                        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)elements.Length * sizeof(uint), buffer, BufferUsageARB.StaticDraw);

                    _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
                    _gl.EnableVertexAttribArray(0);
                }
                _gl.BindVertexArray(0);

                _screenVao = _gl.GenVertexArray();

                _gl.BindVertexArray(_screenVao);
                {
                    var vbo = _gl.GenBuffer();
                    _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                    fixed (float* buffer = frameVertices)
                        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)frameVertices.Length * sizeof(float), buffer, BufferUsageARB.StaticDraw);

                    var ebo = _gl.GenBuffer();

                    _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
                    fixed (uint* buffer = elements)
                        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)elements.Length * sizeof(uint), buffer, BufferUsageARB.StaticDraw);

                    _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
                    _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

                    _gl.EnableVertexAttribArray(0);
                    _gl.EnableVertexAttribArray(1);
                }
                _gl.BindVertexArray(0);
            }

            _screenBuffer = _gl.CreateFramebuffer();

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _screenBuffer);
            {
                _texture = _gl.GenTexture();

                _gl.BindTexture(TextureTarget.Texture2D, _texture);
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 40, 30, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (void*)0);
                _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)GLEnum.Linear);
                _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)GLEnum.Nearest);
                _gl.BindTexture(TextureTarget.Texture2D, 0);

                _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texture, 0);

                if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
                    Console.WriteLine("FB IS NO READY");
            }
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _program = new ShaderProgram(_gl, screenVertex, screenFragment);
            _buffer = new ShaderProgram(_gl, framebufferVertex, frameBufferFragment);

            _gl.Enable(EnableCap.CullFace);
            _gl.CullFace(TriangleFace.Back);
            _gl.FrontFace(FrontFaceDirection.Ccw);
        }


        static void Update(double time)
        {
            _frameCounter++;

            _controller.Update((float)time);
        }

        static unsafe void Render(double time)
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _screenBuffer);
            _gl.Viewport(0, 0, 40, 30);

            _gl.Clear(ClearBufferMask.ColorBufferBit);

            _program.UseProgram();
            _program.SetFloat3(_program.GetLocation("color"), new Vector3D<float>(1, 1, 1));

            _timer += time * 10;

            samples[_frameCounter % (ulong)samples.Length] = (float)time;


            _program.SetMatrix4(_program.GetLocation("local"), Matrix4x4.CreateRotationZ((float)_timer * MathF.PI / 180));

            _gl.BindVertexArray(_scene);

            _gl.DrawElements(PrimitiveType.Triangles, (uint)elements.Length, DrawElementsType.UnsignedInt, (void*)0);

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)_size.X, (uint)_size.Y);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _buffer.UseProgram();
            _gl.BindVertexArray(_screenVao);

            _gl.BindTexture(TextureTarget.Texture2D, _texture);

            _gl.DrawElements(PrimitiveType.Triangles, (uint)elements.Length, DrawElementsType.UnsignedInt, (void*)0);


            ImGui();

            _controller.Render();
        }

        private static unsafe void ImGui()
        {
            //ImGuiNET.ImGui.ShowDemoWindow();

            if (ImGuiNET.ImGui.Begin("Perfomance"))
            {
                ImGuiNET.ImGui.PlotLines("Samples", ref samples[0], samples.Length);

                float ms = MathF.Round(1000f * samples[_frameCounter % (ulong)samples.Length], 2);
                float fps = MathF.Round(1f / samples[_frameCounter % (ulong)samples.Length], 2);

                float average = 0;
                float maxMs = float.MinValue;
                for (int i = 0; i < samples.Length; i++)
                {
                    float curSec = samples[i];
                    average += 1f / curSec;
                    if (maxMs < curSec)
                        maxMs = 1000 * curSec;
                }

                average = MathF.Round(average / samples.Length, 2);
                maxMs = MathF.Round(maxMs, 2);

                ImGuiNET.ImGui.Text(string.Format("ms: {0}{4}max ms:{3}{4}fps: {1}{4}avr fps:{2}", ms, fps, average, maxMs, Environment.NewLine));

                bool vsync = _main.VSync;

                ImGuiNET.ImGui.Checkbox("V sync", ref vsync);

                if (vsync != _main.VSync)
                {
                    if (vsync)
                        samples = new float[30];
                    else
                        samples = new float[300];
                }

                _main.VSync = vsync;

            }
        }

        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _main.Close();
        }

        private static void Resize(Vector2D<int> size)
        {
            _size = size;
        }
    }
}