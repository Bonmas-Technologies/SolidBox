using Silk.NET.Maths;

namespace SolidBoxGE.Core.Render
{
    internal abstract class GERenderer
    {
        public Vector2D<int> ScreenSize { get; set; }

        public abstract unsafe void Init();

        public abstract unsafe void Render(double deltaTime);
    }
}
