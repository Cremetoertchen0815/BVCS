using Microsoft.Xna.Framework;
using System;

namespace Nez
{
    /// <summary>
    /// An exact copy of the MonoGame Vector2 structure, but using a 64-Bit floating point number.
    /// </summary>
    public struct Vector2d
    {
        #region Public Fields

        public double X;

        public double Y;

        #endregion Public Fields

        #region Public Constructors

        public Vector2d(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        #endregion Public Constructors

        #region Public Methods

        public static Vector2d operator +(Vector2d a, Vector2d b) => new Vector2d(a.X + b.X, a.Y + b.Y);

        public static Vector2d operator -(Vector2d a, Vector2d b) => new Vector2d(a.X - b.X, a.Y - b.Y);

        public static bool operator ==(Vector2d a, Vector2d b) => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(Vector2d a, Vector2d b) => !(a == b);

        public static implicit operator Vector2(Vector2d icke) => new Vector2((float)icke.X, (float)icke.Y);

        public static implicit operator Vector2d(Vector2 icke) => new Vector2d(icke.X, icke.Y);

        public static Vector2d operator *(Vector2d a, double b) => new Vector2d(a.X * b, a.Y * b);

        public static Vector2d operator /(Vector2d a, double b) => new Vector2d(a.X / b, a.Y / b);

        public static Vector2d operator *(Vector2d a, Vector2d b) => new Vector2d(a.X * b.X, a.Y * b.Y);

        public static Vector2d Zero => new Vector2d(0, 0);

        public static double Distance(Vector2d a, Vector2d b) => Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));

        public static Vector2d Normalize(Vector2d a)
        {
            double len = Vector2d.Distance(new Vector2d(), a);
            return a / len;
        }

        public Vector2 ToVector2() => new Vector2((float)X, (float)Y);

        public double CrossProduct(Vector2d P1, Vector2d P2) => (P1.X - X) * (P2.Y - Y) - (P2.X - X) * (P1.Y - Y);

        public double DotProduct(Vector2d P1, Vector2d P2) => (P1.X - X) * (P2.X - X) + (P1.Y - Y) * (P2.Y - Y);

        public Vector2d Rotate(float degrees, Vector2d axis) => new Vector2d((X - axis.X) * (float)Math.Cos(degrees / 180.0F * Math.PI) - (Y - axis.Y) * (float)Math.Sin(degrees / 180.0 * Math.PI) + axis.X,
                                (X - axis.X) * (float)Math.Sin(degrees / 180.0F * Math.PI) + (float)Math.Cos(degrees / 180.0 * Math.PI) * (Y - axis.Y) + axis.Y);

        public override bool Equals(object obj) => base.Equals(obj);

        public override int GetHashCode() => base.GetHashCode();

        #endregion Public Methods
    }
}