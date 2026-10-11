using UnityEngine;
using System.Collections.Generic;

public class VoronoiCellCreation : MonoBehaviour
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


    Vector2[] centroids = new Vector2[3];
    Cell limits = new Cell();

    float limit = 100;

    void Start()
    {
        //makePoints();
        centroids[0] = new Vector2(-70, -30);
        centroids[1] = new Vector2(40, -40);
        centroids[2] = new Vector2(-10, 90);


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

        int completed = 0;
        foreach (Vector2 currentPoint in centroids)
        {
            Cell cell = new Cell { boundaries = limits.boundaries };

            foreach (Vector2 point in centroids) {
                if(point == currentPoint)
                {
                    continue;
                }
                print("comparing " + currentPoint + " with " + point);

                Vector2 midpoint = (currentPoint + point) / 2;
                Vector2 difference = currentPoint - point;     
                Vector2 perpendicular = new Vector2(-difference.y, difference.x);
                perpendicular.Normalize();

                Vector2 bisectorPos = midpoint + (4 * limit * perpendicular);
                Vector2 bisectorNeg = midpoint - (4 * limit * perpendicular);


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
                        fullBisector.end = (Vector2)secondIntersection;
                        split2 = i;
                    }

                }

                if (split1 != -1 && split2 != -1 && split1 != split2)
                {
                    Cell subCell = new Cell { boundaries = splitCell(fullBisector, cell, split1, split2, true) };

                    if (inCell(currentPoint, subCell))
                    {
                        cell.boundaries = subCell.boundaries;
                    }
                    else
                    {
                        cell.boundaries = splitCell(fullBisector, cell, split1, split2, false);
                    }
                    foreach (Line l in cell.boundaries)
                    {
                        print("segment: " + l.start + " " + l.end);
                    }
                }
            }
            cell.centroid = currentPoint;
            cells[completed] = cell;
            completed++;
        }
        buildStreets(cells);
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
    

    private void buildStreets(Cell[] cells)
    {
        Color r = Color.red;
        Color g = Color.green;
        Color b = Color.blue;

        Color[] colors = new Color[3];
        colors[0] = r;
        colors[1] = g;
        colors[2] = b;

        int count = 0;

        foreach (Cell c in cells)
        {               
            GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);// make ovbious
            structure.transform.localScale = new Vector3(3, 20, 3);
            structure.transform.position = new Vector3(c.centroid.x, 10, c.centroid.y);
            structure.name = "troid"; 
            
            Renderer objRenderer = structure.GetComponent<Renderer>();

            if (objRenderer != null)
            {
                objRenderer.material.color = colors[count];

            }

            foreach (Line l in c.boundaries)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 start = new Vector3(l.start.x, 3, l.start.y);
                Vector3 end = new Vector3(l.end.x, 3, l.end.y);

                Vector3 direction = end - start;
                cube.transform.position = start + (direction / 2f);
                cube.transform.localScale = new Vector3(2, 2, direction.magnitude);
                if (direction != Vector3.zero)
                {
                    cube.transform.rotation = Quaternion.LookRotation(direction);
                }
                Renderer o = cube.GetComponent<Renderer>();

                if (o != null)
                {
                    o.material.color = colors[count];

                }

            }
            count++;
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


    private Line[] splitCell(Line bisection, Cell cell, int split1, int split2, bool rightSplit)
    {
        List<Line> newCell = new List<Line>();
        if (rightSplit)
        {
            newCell.Add(new Line(cell.boundaries[split2].start, bisection.end));
            newCell.Add(bisection);
            newCell.Add(new Line(bisection.start, cell.boundaries[split1].end));
        }
        else
        {
            newCell.Add(new Line(cell.boundaries[split2].start, bisection.end));
            newCell.Add(bisection);
            newCell.Add(new Line(bisection.start, cell.boundaries[split1].end));
        }

        int startIndex = rightSplit ? split1 : split2;
        int endIndex = rightSplit ? split2 : split1;
        startIndex = (startIndex + 1) % cell.boundaries.Length;

        for (int i = startIndex; i % cell.boundaries.Length != endIndex; i++)
        {
            newCell.Add(cell.boundaries[i % cell.boundaries.Length]);
        }

        return newCell.ToArray();
    }


    private bool onLine(Vector2 point, Line line)
    {
        float a1 = line.start.y - line.end.y;
        float b1 = line.end.x - line.start.x;
        float c1 = a1 * line.start.x + b1 * line.start.y;

        if(c1 - (a1 * point.x + b1 * point.y) <= 0.001)
        {
            return true;
        }

        return false;
    }

    private Vector2? intersect(Line a, Line b)
    {
        float a1 = a.start.y - a.end.y;
        float b1 = a.end.x - a.start.x;
        float c1 = a1 * a.start.x + b1 * a.start.y;
        //print(a1 + "x + " + b1 + "y = " + c1);

        float a2 = b.start.y - b.end.y;
        float b2 = b.end.x - b.start.x;
        float c2 = a2 * b.start.x + b2 * b.start.y;
        //print(a2 + "x + " + b2 + "y = " + c2);

        float det = (a1 * b2) - (a2 * b1);

        if (det == 0)        
        {
            return null;
        }

        float x = (b2 * c1 - b1 * c2) / det;
        float y = (a1 * c2 - a2 * c1) / det;
        Vector2 intersection = new Vector2(x, y);

        bool isInBounds = inBounds(intersection, a) && inBounds(intersection, b);

        if (isInBounds)
        {
            return intersection;
        }

        return null;
    }

    private bool inBounds(Vector2 point, Line line)
    {
        float error = 0.001f;
        float minX = Mathf.Min(line.end.x, line.start.x) - error;
        float maxX = Mathf.Max(line.end.x, line.start.x) + error;
        float minY = Mathf.Min(line.end.y, line.start.y) - error;
        float maxY = Mathf.Max(line.end.y, line.start.y) + error;

        return (point.x >= minX && point.x <= maxX) && (point.y >= minY && point.y <= maxY);
    }

}
