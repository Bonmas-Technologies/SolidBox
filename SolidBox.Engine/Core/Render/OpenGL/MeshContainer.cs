using Silk.NET.Assimp;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;

namespace SolidBoxGE.Core.Render.OpenGL
{
    internal class ModelContainer
    {

    }

    internal class MeshContainer : IDisposable
    {
        public VertexContainer[] vertices;
        public uint[] indices;
        public GLTexture[] textures;

        private GL _gl;

        private uint _vao, _vbo, _ebo;

        public MeshContainer(GL gl)
        {
            _gl = gl;
        }

        public void SetupMesh()
        {
            if (_vao == 0)
                _vao = _gl.CreateVertexArray();

            _gl.BindVertexArray(_vao);



            _gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            _gl.BindVertexArray(_vao);

            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);

            _gl.BindVertexArray(0);

            _gl.DeleteVertexArray(_vao);
        }
    }

    internal struct TextureContainer
    {
        public uint id;
        public string type;
    }

    internal struct VertexContainer
    {
        public Vector3D<float> position;
        public Vector3D<float> normal;
        public Vector2D<float> uv;

        public VertexContainer(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }
}
