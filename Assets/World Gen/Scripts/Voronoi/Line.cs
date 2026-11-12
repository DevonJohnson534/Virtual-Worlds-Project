using UnityEngine;

namespace Voronoi
{
    public class Line
    {
        public Vector2 start;
        public Vector2 end;

        private float length = -1;
        private Vector2 direction = Vector2.zero;

        public Line(Vector2 p1, Vector2 p2)
        {
            start = p1;
            end = p2;
        }


        public float getLength()
        {
            if(length == -1)
            {
                length = Vector2.Distance(start, end);
            }

            return length;
        }


        public Vector2 getDirection()
        {
            if(direction == Vector2.zero)
            {
                direction = (end - start).normalized; // idk if subtrction sohuld be the other way around to point from start 2 end
            }

            return direction;
        }


        public Vector2 angleBisector(Line nextLine) // "next line" meaning this.end = nextLine.start
        {
            Vector2 vertex = end;
		    Vector2 dir1 = (start - vertex).normalized;
		    Vector2 dir2 = (nextLine.end - vertex).normalized;
		    Vector2 angleBisector = (dir1 + dir2).normalized;

            return angleBisector;
        }


        public Vector2? intersection(Line other)
        {
            float a1 = start.y - end.y;
            float b1 = end.x - start.x;
            float c1 = a1 * start.x + b1 * start.y;

            float a2 = other.start.y - other.end.y;
            float b2 = other.end.x - other.start.x;
            float c2 = a2 * other.start.x + b2 * other.start.y;

            float det = (a1 * b2) - (a2 * b1);

            if (det == 0)
            {
                return null; // colinear edge case; i dont care.
            }

            float x = (b2 * c1 - b1 * c2) / det;
            float y = (a1 * c2 - a2 * c1) / det;
            Vector2 intersection = new Vector2(x, y);

            bool isInBounds = this.inBounds(intersection) && other.inBounds(intersection);

            if (isInBounds)
            {
                return intersection;
            }
            return null;
        }


        public bool inBounds(Vector2 point)
        {
            float error = 0.001f;
            float minX = Mathf.Min(end.x, start.x) - error;
            float maxX = Mathf.Max(end.x, start.x) + error;
            float minY = Mathf.Min(end.y, start.y) - error;
            float maxY = Mathf.Max(end.y, start.y) + error;

            return (point.x >= minX && point.x <= maxX) && (point.y >= minY && point.y <= maxY);
        }


        public bool onLine(Vector2 point)
        {
            float a1 = start.y - end.y;
            float b1 = end.x - start.x;
            float c1 = a1 * start.x + b1 * start.y;

            if (Mathf.Abs(Mathf.Abs(c1) - Mathf.Abs(a1 * point.x + b1 * point.y)) <= 0.001)
            {
                return true;
            }

            return false;
        }

    }
}
