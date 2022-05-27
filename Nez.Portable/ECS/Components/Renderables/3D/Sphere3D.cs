using Microsoft.Xna.Framework;
using System;


namespace Nez
{
    public class Sphere3D : GeometricPrimitive3D
    {
        public Sphere3D() : this(5, Color.Red)
        {
        }

        public Sphere3D(int tessellation, Color color)
        {
            float radius = 0.5f;

            int latitudeBands = tessellation;
            int longitudeBands = tessellation * 2;
            var normal = new Vector3();
            var uv = new Vector3();

            for (int latNumber = 0; latNumber <= latitudeBands; latNumber++)
            {
                float theta = latNumber * (float)Math.PI / latitudeBands;
                float sinTheta = (float)Math.Sin(theta);
                float cosTheta = (float)Math.Cos(theta);

                for (int longNumber = 0; longNumber <= longitudeBands; longNumber++)
                {
                    float phi = longNumber * 2.0f * (float)Math.PI / longitudeBands;
                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);

                    normal.X = cosPhi * sinTheta;
                    normal.Y = cosTheta;
                    normal.Z = sinPhi * sinTheta;
                    uv.X = 1.0f - (longNumber / (float)longitudeBands);
                    uv.Y = (latNumber / (float)latitudeBands);

                    _vertices.Add(new VertexPositionColorNormal(normal * radius, color, normal));
                }
            }

            for (int latNumber = 0; latNumber < latitudeBands; latNumber++)
            {
                for (int longNumber = 0; longNumber < longitudeBands; longNumber++)
                {
                    int first = (latNumber * (longitudeBands + 1)) + longNumber;
                    int second = first + longitudeBands + 1;

                    AddIndex(first);
                    AddIndex(second);
                    AddIndex(first + 1);

                    AddIndex(second);
                    AddIndex(second + 1);
                    AddIndex(first + 1);
                }
            }

            InitializePrimitive();
        }
    }
}