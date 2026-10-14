using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class VoronoiCellCreation : MonoBehaviour
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
    public struct Cell
    {
        public Vector2 centroid;
        public Line[] boundaries;
    }


    Vector2[] centroids = new Vector2[100];
    Cell limits = new Cell();

    float limit = 300;

    void Start()
    {
        makePoints();
        //centroids[0] = new Vector2(-93.87712f, 42.48099f);
        //centroids[1] = new Vector2(-22.30507f, 53.53791f);
        //centroids[0] = new Vector2(4.292381f, 52.5742f);
        //centroids[1] = new Vector2(3.262054f, 8.824905f);
        //centroids[2] = new Vector2(31.63235f, -9.683861f);


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

                Vector2 midpoint = (currentPoint + point) / 2;
                Vector2 difference = currentPoint - point;     
                Vector2 perpendicular = new Vector2(-difference.y, difference.x);
                perpendicular.Normalize();

                Vector2 bisectorPos = midpoint + (4 * limit * perpendicular);
                Vector2 bisectorNeg = midpoint - (4 * limit * perpendicular);

                Line bisector = new Line(bisectorPos, bisectorNeg);

                List<Vector2> bisectorIntersections = new List<Vector2>(2);
                List<int >intersectedIndicies = new List<int>(2);

                for (int i = 0; i < cell.boundaries.Length; i++)
                {
                    Line cellBoundary = cell.boundaries[i];
                    Vector2? firstIntersection = intersect(bisector, cellBoundary);
                    Vector2? secondIntersection = intersect(bisector, cellBoundary);

                    if (firstIntersection.HasValue)
                    {
                        bisectorIntersections.Insert(0, (Vector2)firstIntersection);
                        intersectedIndicies.Insert(0, i);
                    }

                    if (secondIntersection.HasValue)
                    {
                        bisectorIntersections.Add((Vector2)secondIntersection);
                        intersectedIndicies.Add(i);
                    }
                }

                bisectorIntersections = bisectorIntersections.Distinct().ToList();

                if(bisectorIntersections.Count == 2) {
                    Line newSegment = new Line(bisectorIntersections[0], bisectorIntersections[1]);

                    int split1 = intersectedIndicies[0];
                    int split2 = intersectedIndicies[1];
                    Cell subCell = new Cell { boundaries = splitCell(newSegment, cell, split1, split2, true) };

                    if (inCell(currentPoint, subCell))
                    {
                        cell.boundaries = subCell.boundaries;
                    }
                    else
                    {
                        cell.boundaries = splitCell(newSegment, cell, split1, split2, false);
                    }
                    print(currentPoint);
                    foreach (Line l in cell.boundaries)
                    {
                        print(l.start + " " + l.end);
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
        Color[] colors = new Color[3];
        colors[0] = Color.red;
        colors[1] = Color.green;
        colors[2] = Color.blue;
        int count = 0;

        foreach (Cell c in cells)
        {               
            GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);// make ovbious
            structure.transform.localScale = new Vector3(2, 1, 2);
            structure.transform.position = new Vector3(c.centroid.x, 1, c.centroid.y);
            structure.name = "troid" + count; 
            
            Color color = Random.ColorHSV(); 
            Renderer objRenderer = structure.GetComponent<Renderer>();

            if (objRenderer != null)
            {
                objRenderer.material.color = color;

            }
            int wallcount = 0;
            foreach (Line l in c.boundaries)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 start = new Vector3(l.start.x, 3, l.start.y);
                Vector3 end = new Vector3(l.end.x, 3, l.end.y);

                Vector3 direction = end - start;
                cube.transform.position = start + (direction / 2f);
                cube.transform.localScale = new Vector3(2, 2, direction.magnitude);
                cube.name = "wall" + wallcount + "of cell " + count;

                if (direction != Vector3.zero)
                {
                    cube.transform.rotation = Quaternion.LookRotation(direction);
                }
                Renderer o = cube.GetComponent<Renderer>();

                if (o != null)
                {
                    o.material.color = color;

                }
                wallcount ++;
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
            newCell.Add(flipIfNeeded(newCell[newCell.Count - 1], bisection));
            newCell.Add(new Line(bisection.start, cell.boundaries[split1].end));
        }
        else
        {
            newCell.Add(new Line(cell.boundaries[split1].start, bisection.start));
            newCell.Add(flipIfNeeded(newCell[newCell.Count - 1], bisection));
            newCell.Add(new Line(bisection.end, cell.boundaries[split2].end));
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


    private Line flipIfNeeded(Line a, Line b)
    {
        if(a.end == b.start)
        {
            return b;
        }
    
        return new Line(b.end, b.start);
    }

    private bool onLine(Vector2 point, Line line)
    {
        float a1 = line.start.y - line.end.y;
        float b1 = line.end.x - line.start.x;
        float c1 = a1 * line.start.x + b1 * line.start.y;

        if(Mathf.Abs(Mathf.Abs(c1) - Mathf.Abs(a1 * point.x + b1 * point.y)) <= 0.001)
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
