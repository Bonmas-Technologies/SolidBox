using System;
using System.Collections.Generic;
using System.Text;

namespace SolidBoxGE.Core.API
{
    internal static class ECS
    {
        private const int initComponents = 100;

        public static List<TransformComponent> transforms;
        public static List<CameraComponent> cameras;
        public static List<LuaComponent> scripts;
        public static List<SpriteComponent> sprites;

        static ECS()
        {
            transforms = new List<TransformComponent>(initComponents);
            cameras = new List<CameraComponent>(initComponents);
            scripts = new List<LuaComponent>(initComponents);
            sprites = new List<SpriteComponent>(initComponents);
        }
    }
}
