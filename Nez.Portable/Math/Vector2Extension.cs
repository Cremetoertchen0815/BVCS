using Microsoft.Xna.Framework;
using System;

namespace Nez
{
    public static class Vector2Extension
    {

        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float sin = (float)Math.Sin(degrees);
            float cos = (float)Math.Cos(degrees);

            float tx = v.X;
            float ty = v.Y;
            v.X = (cos * tx) - (sin * ty);
            v.Y = (sin * tx) + (cos * ty);
            return v;
        }
    }
}