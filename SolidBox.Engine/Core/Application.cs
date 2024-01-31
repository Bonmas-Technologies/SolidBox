using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using SolidBox.Engine.Core.Logic;
using SolidBox.Engine.Core.Renderer.OpenGL;
using SolidBox.Engine.IO;
using System;

namespace SolidBox.Engine.Core
{
    public class Application : IDisposable
    {
        private GL _gl;
        private IInputContext _input;
        private IWindow _window;

        private ImGuiController _imgui;
        private uint _sceneVao;
        private uint _screenVao;


        private uint _screenBuffer;
        private uint _screenTexture;
        private ShaderProgram _program;
        private ShaderProgram _buffer;

        public int Width = 80;
        public int Height = 60;

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

        private static Camera _cam;
        private float[] samples = new float[60];
        private double _timer;
        private ulong _frameCounter;

        public Application()
        {
            _window       = Window.Create(WindowOptions.Default);
            _window.Title = "SolidBox GE [Early]";
            _window.Size  = new Vector2D<int>(Width * 10, Height * 10);
            _window.VSync = true;
        }

        public void Run()
        {
            _window.Load += Setup;
            _window.Update += EngineUpdate;
            _window.Render += EngineRender;
            //_window.Resize += Resize;

            _window.Run();
        }

        public void Dispose()
        {

        }

        private void Setup()
        {
            // RenderAPI Load
            _cam = new Camera(new Vector3D<float>(0, 0, 3), Width / Height);

            _window.Center();

            _gl = _window.CreateOpenGL();
            _input = _window.CreateInput();
            _imgui = new ImGuiController(_gl, _window, _input);

            SetupGL();
        }

        private unsafe void SetupGL()
        {
            _gl.ClearColor(0.3906f, 0.5802f, 0.9257f, 1);

            _sceneVao = _gl.GenVertexArray();

            _gl.BindVertexArray(_sceneVao);
            {
                var vbo = _gl.GenBuffer();
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
                fixed (float* buffer = vertices)
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)vertices.Length * sizeof(float), buffer, BufferUsageARB.StaticDraw);

                var ebo = _gl.GenBuffer();
                _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
                fixed (uint* buffer = elements)
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)elements.Length * sizeof(uint), buffer, BufferUsageARB.StaticDraw);

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
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)frameVertices.Length * sizeof(float), buffer, BufferUsageARB.StaticDraw);

                var ebo = _gl.GenBuffer();

                _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
                fixed (uint* buffer = elements)
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)elements.Length * sizeof(uint), buffer, BufferUsageARB.StaticDraw);

                _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
                _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

                _gl.EnableVertexAttribArray(0);
                _gl.EnableVertexAttribArray(1);
            }
            _gl.BindVertexArray(0);

            _screenBuffer = _gl.CreateFramebuffer();

            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _screenBuffer);
            {
                _screenTexture = _gl.GenTexture();

                _gl.BindTexture(TextureTarget.Texture2D, _screenTexture);
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, (uint)Width, (uint)Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (void*)0);
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

        private void EngineUpdate(double deltaTime)
        {
            ApplicationUpdate(deltaTime); // call application update
            // process ECS and logic

            _imgui.Update((float)deltaTime);
            _frameCounter++;
        }

        private unsafe void EngineRender(double deltaTime) 
        {
            // render api calls

            // .render scene
            // .run postprocess

            _timer += deltaTime * 10;
            samples[_frameCounter % (ulong)samples.Length] = (float)deltaTime;

            // pass 1
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _screenBuffer);
            _gl.Viewport(0, 0, (uint)Width, (uint)Height);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _gl.Enable(EnableCap.DepthTest);
            _gl.BindVertexArray(_sceneVao);

            _program.UseProgram();
            _program.SetFloat3(_program.GetLocation("color"), new Vector3D<float>(1, 1, 1));
            _program.SetMatrix4(_program.GetLocation("local"),  Matrix4X4.CreateRotationZ<float>((float)_timer * MathF.PI / 180f) * Matrix4X4.CreateRotationY<float>((float)_timer * 20f * MathF.PI / 180f));
            _program.SetMatrix4(_program.GetLocation("world"), _cam.GetViewMatrix());
            _program.SetMatrix4(_program.GetLocation("view"), _cam.GetProjectionMatrix());

            _gl.DrawElements(PrimitiveType.Triangles, (uint)elements.Length, DrawElementsType.UnsignedInt, (void*)0);

            // pass 2
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)(Width * 10), (uint)(Height * 10));
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _gl.Disable(EnableCap.DepthTest);
            _gl.BindVertexArray(_screenVao);

            _buffer.UseProgram();
            _gl.BindTexture(TextureTarget.Texture2D, _screenTexture);

            _gl.DrawElements(PrimitiveType.Triangles, (uint)elements.Length, DrawElementsType.UnsignedInt, (void*)0);

            ImGuiSetup();

            _imgui.Render();
        }

        private unsafe void ImGuiSetup()
        {
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

                bool vsync = _window.VSync;

                ImGuiNET.ImGui.Checkbox("V sync", ref vsync);

                if (vsync != _window.VSync)
                    if (vsync)
                        samples = new float[30];
                    else
                        samples = new float[300];

                _window.VSync = vsync;
            }
        }


        protected virtual void ApplicationUpdate(double deltaTime)
        {

        }
    }
}
