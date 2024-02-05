using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SolidBoxGE.Core.Render.OpenGL;
using System;
using SolidBoxGE.Core.Render;

namespace SolidBoxGE.Core
{
    public class Application : IDisposable
    {
        private GL _gl;
        private IInputContext _input;
        private IWindow _window;

        private GERenderer _renderer;

        public int Width = 800;
        public int Height = 600;

        public Application()
        {
            _window = Window.Create(WindowOptions.Default);
            _window.Title = "SolidBox GE [Early]";
            _window.Size = new Vector2D<int>(Width, Height);
            _window.VSync = true;

        }

        public void Run()
        {
            _window.Load += Setup;
            _window.Update += EngineUpdate;
            _window.Resize += OnResize;
            _window.Run();
        }

        private void OnResize(Vector2D<int> d)
        {
            Width = d.X;
            Height = d.Y;
            _renderer.ScreenSize = d;
        }

        public void Dispose()
        {

        }

        private unsafe void Setup()
        {
            _window.Center();
            _gl = _window.CreateOpenGL();
            _input = _window.CreateInput();

            _renderer = new OpenGLRenderer(_gl);
            _renderer.Init();
            _window.Render += _renderer.Render;
            _renderer.ScreenSize = _window.Size;
        }

        private void EngineUpdate(double deltaTime)
        {
            ApplicationUpdate(deltaTime); // call application update
            // process ECS and logic
        }

        protected virtual void ApplicationUpdate(double deltaTime)
        {

        }
    }
}
