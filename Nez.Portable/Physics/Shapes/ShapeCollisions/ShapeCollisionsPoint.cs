using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
    public static partial class ShapeCollisions
    {
        public static bool PointToCircle(Vector2 point, Circle circle, out CollisionResult result)
        {
            result = new CollisionResult();

            // avoid the square root until we actually need it
            float distanceSquared = Vector2.DistanceSquared(point, circle.position);
            float sumOfRadii = 1 + circle.Radius;
            bool collided = distanceSquared < sumOfRadii * sumOfRadii;
            if (collided)
            {
                result.Normal = Vector2.Normalize(point - circle.position);
                float depth = sumOfRadii - Mathf.Sqrt(distanceSquared);
                result.MinimumTranslationVector = -depth * result.Normal;
                result.Point = circle.position + result.Normal * circle.Radius;

                return true;
            }

            return false;
        }


        public static bool PointToBox(Vector2 point, Box box, out CollisionResult result)
        {
            result = new CollisionResult();

            if (box.ContainsPoint(point))
            {
                // get the point in the space of the Box
                result.Point = box.bounds.GetClosestPointOnRectangleBorderToPoint(point, out result.Normal);
                result.MinimumTranslationVector = point - result.Point;

                return true;
            }

            return false;
        }


        public static bool PointToPoly(Vector2 point, Polygon poly, out CollisionResult result)
        {
            result = new CollisionResult();

            if (poly.ContainsPoint(point))
            {
                var closestPoint = Polygon.GetClosestPointOnPolygonToPoint(poly.Points, point - poly.position,
                    out float distanceSquared, out result.Normal);

                result.MinimumTranslationVector = result.Normal * Mathf.Sqrt(distanceSquared);
                result.Point = closestPoint + poly.position;

                return true;
            }

            return false;
        }
    }
}