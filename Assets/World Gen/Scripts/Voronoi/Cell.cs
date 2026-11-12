using UnityEngine;

namespace Voronoi
{
    public class Cell
    {
        public Line[] boundaries;
        private Vector2? point = null;
        private Vector2? centroid;


        public Cell(Vector2[] corners)
        {
            boundaries = new Line [corners.Length];

            for(int i = 0; i < boundaries.Length; i++)
            {
                int next = (i + 1) % boundaries.Length;
                boundaries[i] = new Line(corners[i], corners[next]);
            }
        }        

        public Cell(Line[] boundaries)
        {
            this.boundaries = boundaries;
        }          
        
        
        public Cell(Line[] boundaries, Vector2 point) : this(boundaries)
        {
            this.point = point;
        }          
        
        public Cell(Vector2[] corners, Vector2 point) : this(corners)
        {
            this.point = point;
        }


        public Vector2 getPoint()
        {
            return point.Value;
        }        
        
        public void setPoint(Vector2 point)
        {
            this.point = point;
        }


        public Vector2 getCenter()
        {
            if (centroid.HasValue)
            {
                return centroid.Value;
            }
            
            Vector2 sum = Vector2.zero;
            
            foreach(Line l in boundaries)
            {
                sum += l.start;
            }
            
            centroid = sum / boundaries.Length;

            return centroid.Value;
        }


        public int longestBorder()
        {
            Line max = boundaries[0];
            int index = 0;

            for(int i = 1; i < boundaries.Length; i++)
            {
                float length =  boundaries[i].getLength();

                if(length > max.getLength())
                {
                    max = boundaries[i];
                    index = i;
                }
            }

            return index;
        }


    }
}
