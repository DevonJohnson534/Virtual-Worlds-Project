using System.Drawing;
using System.Linq;
using UnityEngine;
using static streets;

public class streets : MonoBehaviour
{
    public struct Line
    {
        public Vector2 start;//calculating lines no slope formula 2 avoid division by 0, which is high chance
        public Vector2 end;
        public Line(Vector2 p1, Vector2 p2)
        {
            start = p1;
            end = p2;
        }
    }
    public struct Cell
    {
        public Vector2 centroid;
        public Line[] boundaries;//using arrays instead of lists because im stupid and forgor and dont know what c# is
    }


    Vector2[] centroids = new Vector2[30];
    Cell limits = new Cell();

    float limit = 100;

    void Start()
    {
        makePoints();

        limits.boundaries = new Line[4];// no voroni outside this space. a square for now
        Vector2 corner1 = new Vector2(-limit, -limit); //turn into a fucniton later probably.
        Vector2 corner2 = new Vector2(-limit, limit);
        Vector2 corner3 = new Vector2(limit, limit);
        Vector2 corner4 = new Vector2(limit, -limit);

        limits.boundaries[0] = new Line(corner1, corner2);
        limits.boundaries[1] = new Line(corner2, corner3);
        limits.boundaries[2] = new Line(corner3, corner4);
        limits.boundaries[3] = new Line(corner4, corner1);
        limits.centroid = new Vector2(0, 0); //does not have to be 0,0. can be centered elsewhere.



        Cell[] cells = new Cell[centroids.Length];
        
        foreach (Vector2 currentPoint in centroids)
        {
            GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);// make ovbious
            structure.transform.localScale = new Vector3(3, 20, 3);
            structure.transform.position = new Vector3(currentPoint.x, 10, currentPoint.y);

            Cell cell = new Cell();
            cell.boundaries = limits.boundaries.ToArray();

            foreach (Vector2 point in centroids) {
                if(point == currentPoint)
                {
                    continue;
                }

                Vector2 midpoint = (currentPoint + point) / 2;
                Vector2 difference = currentPoint - point;     
                Vector2 perpendicular = new Vector2(-difference.y, difference.x);

                Vector2 bisectorPos = midpoint + ((limit + 1) * perpendicular);
                Vector2 bisectorNeg = midpoint + (-(limit + 1) * perpendicular);

                Line bisector1 = new Line(midpoint, bisectorPos);
                Line bisector2 = new Line(midpoint, bisectorNeg);

                Line fullBisector = new Line();
                int split1 = -1;
                int split2 = -1;

                Vector2? intersection1 = null, intersection2 = null;

                for (int i = 0; i < cell.boundaries.Length; i++)
                {
                    Line cellBoundary = cell.boundaries[i];
                    Vector2? firstIntersection = intersect(bisector1, cellBoundary);
                    Vector2? secondIntersection = intersect(bisector2, cellBoundary);

                    if (firstIntersection.HasValue)
                    {
                        fullBisector.start = (Vector2) firstIntersection;
                        split1 = i;
                    }
                    if (secondIntersection.HasValue)
                    {
                        fullBisector.end = (Vector2) secondIntersection;
                        split2 = i;
                    }
                }

                if (split1 != -1 && split2 != -1 && split1 != split2)
                {
                    Cell subCell = splitCell(fullBisector, cell, split1, split2);
                    if(inCell(currentPoint, subCell))
                    {
                        cell = subCell;
                    }
                    else
                    {
                        cell = splitCell(fullBisector, cell, split2, split1);
                    }
                }
            }
        }
    }

    private void makePoints()
    {
        for (int i = 0; i < centroids.Length; i++)
        {
            float randX = UnityEngine.Random.Range(-limit, limit);
            float randZ = UnityEngine.Random.Range(-limit, limit);

            centroids[i] = new Vector2(randX, randZ);
        }
    }


    private bool inCell(Vector2 point, Cell cell)
    {
        
        Vector2 mid = (cell.boundaries[0].start + cell.boundaries[0].start) / 2;
        Vector2 direction = point - mid;
        Vector2 farEnd = point + limit * direction;

        Line ray = new Line(point, farEnd);

        int crosses = 0;

        for (int i = 0; i < cell.boundaries.Length; i ++)
        {
            if(onLine(point, cell.boundaries[i]))
            {
                return true;
            }

            if(intersect(ray, cell.boundaries[i]).HasValue)
            {
                crosses += 1;
                if(crosses > 1)
                {
                    return false;
                }
            }
        }

        return crosses == 1;

    }


    private Cell splitCell(Line bisection, Cell cell, int split1, int split2)
    {
        Line[] newCell = new Line[cell.boundaries.Length  + 2];
        newCell[0] = bisection;

        bool rightSplit = onLine(bisection.start, cell.boundaries[split1]); //determines counterclockwise vs clockwise cell relative to bisection start point.
        int startIndex = rightSplit ? split1 : split2;

        for (int i = startIndex; i < cell.boundaries.Length + startIndex; i++)
        {
            if (i == split1)
            {
                newCell[i - startIndex] = determineSplitLine(bisection, cell.boundaries[split1], rightSplit);
            }
            else if (i == split2)
            {
                print(split2 + " "+ newCell.Length);
                newCell[i - startIndex] = determineSplitLine(bisection, cell.boundaries[split2], rightSplit);
            }
            else
            {
                newCell[i - startIndex] = cell.boundaries[i % cell.boundaries.Length];
            }
        }

        return new Cell {boundaries = newCell };
    }


    private Line determineSplitLine(Line bisector, Line split, bool rightSplit) // already assumed that these lines touch
    {
        bool splitStart = onLine(bisector.start, split);

        if (rightSplit && splitStart)
        {
            return new Line(bisector.start, split.end);
        }
        else if (rightSplit && !splitStart)
        {
            return new Line(split.start, bisector.end);
        }

        else if (!rightSplit && splitStart)
        {
            return new Line(split.start, bisector.start);
        }
        else // (!rightSplit && !splitStart)
        {
            return new Line(bisector.end, split.end);
        }
    }


    private bool onLine(Vector2 point, Line line)
    {
        float a1 = line.start.y - line.end.y;
        float b1 = line.end.x - line.start.x;
        float c1 = a1 * line.start.x + b1 * line.start.y;

        if(c1 == a1 * point.x + b1 * point.y)
        {
            return true;
        }

        return false;
    }

    private Vector2? intersect(Line a, Line b) //from geeksforgeeks.org. Intersections but also avoiding diviision by zero
    {
        float a1 = a.start.y - a.end.y;
        float b1 = a.end.x - a.start.x;
        float c1 = a1 * a.start.x + b1 * a.start.y;

        float a2 = b.start.y - b.end.y;
        float b2 = b.end.x - b.start.x;
        float c2 = a2 * b.start.x + b2 * b.start.y;

        float det = a1 * b2 - a2 * b1;

        if (det == 0)        
        {
            return null;
        }

        float xNum = b2 * c1 - b1 * c2;
        float yNum = a1 * c2 - a2 * c1;
        Vector2 intersection = new Vector2(xNum, yNum);

        // Make the denominator positive so that all comparisons use the same direction.
        if (det < 0)
        {
            det = -det;
            intersection.x = -xNum;
            intersection.y = -yNum;
        }

        // Check whether the intersection point lies
        // inside both line segments.
        bool isInBounds = inBounds(intersection, a, det) && inBounds(intersection, b, det);

        if (isInBounds)
        {
            return new Vector2(xNum, yNum);
        }
        return null;
    }

    private bool inBounds(Vector2 point, Line line, float det)
    {
        float minX1 = Mathf.Min(line.end.x, line.start.x) * det;
        float minY1 = Mathf.Min(line.end.y, line.start.y) * det;
        float maxX1 = Mathf.Max(line.end.x, line.start.x) * det;
        float maxY1 = Mathf.Max(line.end.y, line.start.y) * det;

        return point.x >= minX1 && point.x <= maxX1 && point.y >= minY1 && point.y <= maxY1;
    }

}
