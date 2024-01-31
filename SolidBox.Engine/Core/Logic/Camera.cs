using System;
using Silk.NET.Maths;

namespace SolidBox.Engine.Core.Logic
{
    internal class Camera
    {
        private Vector3D<float> _front = -Vector3D<float>.UnitZ;

        private Vector3D<float> _up = Vector3D<float>.UnitY;

        private Vector3D<float> _right = Vector3D<float>.UnitX;

        private float _pitch;

        private float _yaw = -MathHelper.PiOver2;

        private float _fov = MathHelper.PiOver4;

        public Camera(Vector3D<float> position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        public Vector3D<float> Position { get; set; }

        public float AspectRatio { private get; set; }

        public Vector3D<float> Front => _front;

        public Vector3D<float> Up => _up;

        public Vector3D<float> Right => _right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                var angle = Math.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = Math.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4X4<float> GetViewMatrix()
        {
            return Matrix4X4.CreateLookAt(Position, Position + _front, _up);
        }

        public Matrix4X4<float> GetProjectionMatrix()
        {
            return Matrix4X4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        private void UpdateVectors()
        {
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            _front = Vector3D.Normalize(_front);

            _right = Vector3D.Normalize(Vector3D.Cross(_front, Vector3D<float>.UnitY));
            _up = Vector3D.Normalize(Vector3D.Cross(_right, _front));
        }
    }
}
