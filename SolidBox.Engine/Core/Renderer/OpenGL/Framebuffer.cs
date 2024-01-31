using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolidBox.Engine.Core.Renderer.OpenGL
{
    internal class Framebuffer : IDisposable
    {
        private GL _context;
        private uint _buffer;
        private Vector2D<int> _viewport;


        public Framebuffer()
        {

        }


        public void UseFramebuffer()
        {
            _context.BindFramebuffer(FramebufferTarget.Framebuffer, _buffer);
            _context.Viewport(0, 0, (uint)_viewport.X, (uint)_viewport.Y);

        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
