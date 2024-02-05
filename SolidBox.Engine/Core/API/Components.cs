using System;
using Silk.NET.Maths;
using SolidBoxGE.Core.Render.OpenGL;

namespace SolidBoxGE.Core.API
{
    internal struct TransformComponent
    {
        public Vector3D<float> position;
        public Vector3D<float> scale;
        public Quaternion<float> rotation;

        public Vector3D<float> front;
        public Vector3D<float> up;
        public Vector3D<float> right;

        public void UpdateVectors()
        {
            var mat = Matrix4X4.CreateFromQuaternion<float>(rotation);

            front = To3D(-Vector4D<float>.UnitZ * mat);
            up    = To3D(Vector4D<float>.UnitY * mat);
            right = To3D(Vector4D<float>.UnitX * mat);
        }

        private static Vector3D<float> To3D(Vector4D<float> vector) => new Vector3D<float>(vector.X, vector.Y, vector.Z);
    }

    internal struct MeshComponent
    {
        public bool doubleSided;
        public bool lit;
        public MeshContainer mesh;
    }

    internal struct CameraComponent
    {
        public bool main;

        public float nearPlaneClip;
        public float farPlaneClip;

        public bool ortographic;
        public float ortographicSize;

        public float perspectiveFov;
        public float aspectRatio;

        public Matrix4X4<float> GetProjectionMatrix()
        {
            Matrix4X4<float> matrix;

            if (ortographic)
            {
                float width = ortographicSize * aspectRatio;

                matrix = Matrix4X4.CreateOrthographic<float>(
                    width, ortographicSize, nearPlaneClip, farPlaneClip);
            }
            else
            {
                matrix = Matrix4X4.CreatePerspectiveFieldOfView<float>(
                    perspectiveFov, aspectRatio, nearPlaneClip, farPlaneClip);
            }

            return matrix;
        }
    }

    internal struct SpriteComponent
    {
        public bool billboard;
        public bool lit;
        public bool Transparent;
        public Vector3D<float> color;
    }

    internal struct LuaComponent
    {
    }
}
