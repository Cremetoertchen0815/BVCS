using System;

namespace Nez
{
    /// <summary>
    /// Represents a line segment in a lightweight form of a ray with start, direction and length.
    /// </summary>
    public struct BoundRay
    {
        public Vector2d Location;
        public Vector2d Direction;
        public double Length;

        public Line ToLine() => new Line(Location, Location + Direction * Length);

        public BoundRay(Vector2d Location, Vector2d Direction, double Length)
        {
            this.Location = Location;
            this.Direction = Direction;
            this.Length = Length;
        }
        public BoundRay(Vector2d a, Vector2d b, bool isBEndVector = true)
        {
            Location = a;
            Direction = isBEndVector ? b - a : b;
            Length = Math.Sqrt(Math.Pow(Direction.X, 2) + Math.Pow(Direction.Y, 2));
            Direction /= Length;
        }

        /// <summary>
        /// Returns a point along the ray with a given distance to the start.
        /// </summary>
        /// <param name="distance">The distance to the start vector.</param>
        /// <param name="isDistancePercentage">If true, interprets the distance as a ratio, with 0 being the start vector and 1 being the end vector.</param>
        public Vector2d SlideAlong(double distance, bool isDistancePercentage = false) => isDistancePercentage ? Location + Direction * Length * Mathf.Clamp01((float)distance) : Location + Direction * distance;
        public Vector2d End => Location + Direction * Length;
    }
}
