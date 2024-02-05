using Silk.NET.OpenGL;
using StbImageSharp;
using System.IO;

namespace SolidBoxGE.Core.Render.OpenGL
{
    internal class GLTexture
    {
        public uint Handle => _handle;

        private uint _handle;

        static GLTexture()
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
        }

        public GLTexture(uint handle)
        {
            _handle = handle;
        }

        public static unsafe GLTexture LoadTexture2D(GL _gl, string path, GLTextureParameters parameters)
        {
            ImageResult result = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

            uint handle = _gl.GenTexture();

            _gl.BindTexture(TextureTarget.Texture2D, handle);

            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)parameters.wrapS);
            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)parameters.wrapT);
            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)parameters.minFilter);
            _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)parameters.magFilter);

            fixed (byte* ptr = &result.Data[0])
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width, (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

            _gl.GenerateMipmap(TextureTarget.Texture2D);

            _gl.BindTexture(TextureTarget.Texture2D, 0);

            return new GLTexture(handle);
        }
    }

    internal struct GLTextureParameters
    {
        public GLEnum wrapS;
        public GLEnum wrapT;

        public GLEnum minFilter;
        public GLEnum magFilter;

        public static GLTextureParameters Default => new GLTextureParameters()
        {
            wrapS = GLEnum.Repeat,
            wrapT = GLEnum.Repeat,
            minFilter = GLEnum.LinearMipmapLinear,
            magFilter = GLEnum.Linear
        };

        public static GLTextureParameters Pixelated => new GLTextureParameters()
        {
            wrapS = GLEnum.Repeat,
            wrapT = GLEnum.Repeat,
            minFilter = GLEnum.LinearMipmapLinear, // we dont need junky mess of pixels when rendering big texture on small object
            magFilter = GLEnum.Nearest
        };
    }
}
