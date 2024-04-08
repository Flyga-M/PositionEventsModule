using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;

namespace Flyga.PositionEventsModule.Debug.Entity.Primitive
{
    public static class PolygonUtil
    {
        /// <summary>
        /// Calculates the center of the <see cref="RectangleF">BoundingRectangle</see>, that encompasses the 
        /// <paramref name="polygon"/>.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns>The center of the <see cref="RectangleF">BoundingRectangle</see>, that encompasses the 
        /// <paramref name="polygon"/>.</returns>
        public static Vector2 GetCenter(Polygon polygon)
        {
            return polygon.BoundingRectangle.Center;
        }

        /// <summary>
        /// Calculates the <paramref name="polygon"/> recentered around the <paramref name="target"/> position.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="target"></param>
        /// <returns>A new <see cref="Polygon"/>, shaped like the <paramref name="polygon"/>, that is 
        /// centered around the <paramref name="target"/>.</returns>
        public static Polygon CenterAround(Polygon polygon, Vector2 target)
        {
            Vector2 difference = target - GetCenter(polygon);
            return polygon.TransformedCopy(difference, 0, Vector2.One);
        }
    }
}
