using BuildingGen;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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


        public Vector2? getPoint()
        {
            return point;
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


        public bool inCell(Vector2 point)
        {
            Vector2 mid = (boundaries[0].start + boundaries[0].end) / 2;
            Vector2 direction = point - mid;
            Vector2 farEnd = point + boundaries[longestBorderIndex()].getLength() * 2 * direction;

            Line ray = new Line(point, farEnd);

            int crosses = 0;

            for (int i = 0; i < boundaries.Length; i++)
            {
                if (boundaries[i].onLine(point))
                {
                    return true;
                }

                if (ray.intersection(boundaries[i], true).HasValue)
                {
                    crosses += 1;
                    if (crosses > 1)
                    {
                        return false;
                    }
                }
            }

            return crosses == 1;
        }


        public Vector2[] asCorners()
        {
            Vector2[] corners = new Vector2[boundaries.Length];

            for(int i = 0; i < boundaries.Length; i ++)
            {
                corners[i] = boundaries[i].start;
            }
            return corners;
        }

        public Cell splitCell(Line divider, bool clockwise)//divider is a line who's end points are each on a different edge of the cell;
        {
            Vector2[] corners = asCorners();
            Vector2 corner1 = divider.start;
            Vector2 corner2 = divider.end;

            int index1 = -1;
            int index2 = -1;

            for (int i = 0; i < boundaries.Length; i++)
            {
                if (boundaries[i].onLine(corner1))
                {
                    index1 = i;
                }

                if (boundaries[i].onLine(corner2))
                {
                    index2 = i;
                }
            }

            int startIndex = clockwise ? index1 : index2;
            int endIndex = clockwise ? index2 : index1;
            Debug.Log(startIndex);
            Debug.Log(endIndex);
            Vector2[] newCorners = new Vector2[boundaries.Length];

            for (int i = startIndex; i < startIndex + boundaries.Length; i ++)
            {
                int index = i % boundaries.Length;

                newCorners[i - startIndex] = corners[index];

                if (index == index1)
                {
                    newCorners[i - startIndex] = corner1;
                }
                else if (index == index2)
                {
                    newCorners[i - startIndex] = corner2;
                }
            }

            return new Cell(newCorners);
        }


        public int longestBorderIndex()
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

        public float longestBorderLength()
        {
            return boundaries[longestBorderIndex()].getLength();
        }


        public string toString()
        {
            string outStr = "";
            foreach(Line l in boundaries)
            {
                outStr = outStr + l.toString() + ", ";
            }

            return outStr;

        }

    }
}
