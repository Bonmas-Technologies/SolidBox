using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SolidBox.Engine.Core;
using SolidBox.Engine.Core.Logic;
using SolidBox.Engine.IO;
using System.Numerics;
using System.Text;

namespace Test
{
    internal class Program
    {
        private static GL _gl;

        private static Camera _cam;

        private static IWindow _main;
        private static IInputContext _inputContext;
        private static ImGuiController _imGuiController;

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
            2, 3, 1,
            1, 2, 0,
            2, 1, 3,
        };
        private static uint _screenBuffer;
        private static uint _screenTexture;
        private static double _timer;
        private static Vector2 prevMousePosition;

        private static float _speed = 10;


        private const int Width = 100;
        private const int Height = 75;
        private static float[] samples = new float[60];
        private static float _sens = 0.1f;
        private static bool _cursorFixed = false;

        static void Main(string[] args)
        {
            _cam = new Camera(new Vector3D<float>(0, 0, 3), Width / Height);
            _output = Console.Out;
            _counter = new StringBuilder();

            _size = new Vector2D<int>(800, 600);

            _main = Window.Create(WindowOptions.Default with
            {
                Size = _size,
                Title = "SolidBox GE [Early]",
                VSync = true,
                Position = new Vector2D<int>(100,100)
            });

            _main.Load += OnLoad;
            _main.Update += Update;
            _main.Render += Render;
            _main.Resize += Resize;

            _main.Run();
        }


        static unsafe void OnLoad()
        {
            _main.Center();
            _gl = _main.CreateOpenGL();

            _inputContext = _main.CreateInput();

            _imGuiController = new ImGuiController(_gl, _main, _inputContext);

            for (int i = 0; i < _inputContext.Keyboards.Count; i++)
            {
                _inputContext.Keyboards[i].KeyDown += KeyDown;
            }

            for (int i = 0; i < _inputContext.Mice.Count; i++)
                _inputContext.Mice[i].MouseMove += MouseMove;

            _gl.ClearColor(0.3906f, 0.5802f, 0.9257f, 1);

            unsafe
            {
                int major = 0;
                int minor = 0;

                _gl.GetInteger(GetPName.MajorVersion, &major);
                _gl.GetInteger(GetPName.MajorVersion, &minor);

                _main.Title = string.Format("{0} | OpenGL: {1}.{2}", _main.Title, major, minor);

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
                _screenTexture = _gl.GenTexture();

                _gl.BindTexture(TextureTarget.Texture2D, _screenTexture);
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, Width, Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (void*)0);
                _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)GLEnum.Linear);
                _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)GLEnum.Nearest);
                _gl.BindTexture(TextureTarget.Texture2D, 0);

                _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _screenTexture, 0);

                if (_gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
                    Console.WriteLine("FB IS NO READY");
            }
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _program = new ShaderProgram(_gl, ShaderLoader.LoadShader(@"Resources\Shaders\screen.vert"), ShaderLoader.LoadShader(@"Resources\Shaders\screen.frag"));
            _buffer = new ShaderProgram(_gl, ShaderLoader.LoadShader(@"Resources\Shaders\output.vert"), ShaderLoader.LoadShader(@"Resources\Shaders\output.frag"));

            _gl.Enable(EnableCap.CullFace);
            _gl.CullFace(TriangleFace.Back);
            _gl.FrontFace(FrontFaceDirection.Ccw);
        }

        private static void MouseMove(IMouse mouse, Vector2 vector)
        {
            if (_cursorFixed)
                mouse.Cursor.CursorMode = CursorMode.Disabled;
            else
                mouse.Cursor.CursorMode = CursorMode.Normal;

            if (prevMousePosition == Vector2.Zero)
                prevMousePosition = vector;

            Vector2 offset = vector - prevMousePosition;

            offset *= _sens;

            if (_cursorFixed)
            {
                _cam.Pitch -= offset.Y;
                _cam.Yaw += offset.X;
            }

            prevMousePosition = vector;
        }

        static void Update(double time)
        {
            _frameCounter++;

            var keyboard = _inputContext.Keyboards[0];

            if (keyboard.IsKeyPressed(Key.E))
                _cam.Position += Vector3D<float>.UnitY * (float)time * _speed;

            if (keyboard.IsKeyPressed(Key.Q))
                _cam.Position -= Vector3D<float>.UnitY * (float)time * _speed;

            if (keyboard.IsKeyPressed(Key.W))
                _cam.Position += _cam.Front * (float)time * _speed;

            if (keyboard.IsKeyPressed(Key.S))
                _cam.Position -= _cam.Front * (float)time * _speed;

            if (keyboard.IsKeyPressed(Key.D))
                _cam.Position += _cam.Right * (float)time * _speed;

            if (keyboard.IsKeyPressed(Key.A))
                _cam.Position -= _cam.Right * (float)time * _speed;


            _imGuiController.Update((float)time);
        }

        static unsafe void Render(double time)
        {
            _timer += time * 10;
            samples[_frameCounter % (ulong)samples.Length] = (float)time;

            // pass 1
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _screenBuffer);
            _gl.Viewport(0, 0, Width, Height);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _gl.Enable(EnableCap.DepthTest);
            _gl.BindVertexArray(_scene);

            _program.UseProgram();
            _program.SetFloat3(_program.GetLocation("color"), new Vector3D<float>(1, 1, 1));
            _program.SetMatrix4(_program.GetLocation("local"),  Matrix4X4.CreateRotationZ<float>((float)_timer * MathF.PI / 180f) * Matrix4X4.CreateRotationY<float>((float)_timer * 20f * MathF.PI / 180f));
            _program.SetMatrix4(_program.GetLocation("world"), _cam.GetViewMatrix());
            _program.SetMatrix4(_program.GetLocation("view"), _cam.GetProjectionMatrix());

            _gl.DrawElements(PrimitiveType.Triangles, (uint)elements.Length, DrawElementsType.UnsignedInt, (void*)0);

            // pass 2
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)_size.X, (uint)_size.Y);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _gl.Disable(EnableCap.DepthTest);
            _gl.BindVertexArray(_screenVao);

            _buffer.UseProgram();
            _gl.BindTexture(TextureTarget.Texture2D, _screenTexture);

            _gl.DrawElements(PrimitiveType.Triangles, (uint)elements.Length, DrawElementsType.UnsignedInt, (void*)0);

            ImGuiSetup();

            _imGuiController.Render();
        }

        private static unsafe void ImGuiSetup()
        {
            //ImGuiNET.ImGui.ShowDemoWindow();

            if (ImGuiNET.ImGui.Begin("Help"))
                ImGuiNET.ImGui.Text(string.Format("WASD - move{0}EQ - up/down{0}F1 - switch cursor", Environment.NewLine));

            if (ImGuiNET.ImGui.Begin("Perfomance"))
            {
                ImGuiNET.ImGui.PlotLines("Frametime", ref samples[0], samples.Length);

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
                    if (vsync)
                        samples = new float[30];
                    else
                        samples = new float[300];

                _main.VSync = vsync;
            }
        }

        static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            if (key == Key.Escape)
                _main.Close();

            if (key == Key.F1)
                _cursorFixed = !_cursorFixed;
        }

        private static void Resize(Vector2D<int> size)
        {
            _size = size;
        }
    }
}