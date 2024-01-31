using System;

namespace SolidBox.Engine.Core
{
    internal class MathHelper
    {
        internal static float PiOver2 = MathF.PI / 2f;
        internal static float PiOver4 = MathF.PI / 4f;

        internal static float DegreesToRadians(float angle) => MathF.PI / 180f * angle;

        internal static float RadiansToDegrees(float angle) => 180f / MathF.PI * angle;
    }
}