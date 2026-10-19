using UnityEngine;

namespace Voronoi
{
    public struct Line
    {
        public Vector2 start;
        public Vector2 end;
        public Line(Vector2 p1, Vector2 p2)
        {
            start = p1;
            end = p2;
        }
    }
}
