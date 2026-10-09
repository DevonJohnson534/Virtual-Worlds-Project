using System.Linq;
using UnityEngine;

public class streets : MonoBehaviour
{
    public struct Line
    {
        public Vector2 start;//calculating lines no slope formula 2 avoid division by 0, which is high chance
        public Vector2 end;
    }
    public struct Cell
    {
        public Vector2 centroid;
        public Line[] boundaries;//using arrays instead of lists because im stupid and forgor and dont know what c# is
    }


    Vector2[] centroids = new Vector2[6];
    Cell limits = new Cell(); 

    float maxX = 100f;
    float minX = -100f;
    float maxZ = 100f;
    float minZ = -100f;

    void Start()
    {
        makePoints();

        limits.boundaries = new Line[4];// no voroni outside this space. a square for now
        Vector2 corner1 = new Vector2(minX, minZ); //turn into a fucniton later probably.
        Vector2 corner2 = new Vector2(minX, maxZ);
        Vector2 corner3 = new Vector2(maxX, minZ);
        Vector2 corner4 = new Vector2(maxX, maxZ);

        limits.boundaries[0] = connectPoints(corner1, corner2);
        limits.boundaries[1] = connectPoints(corner2, corner3);
        limits.boundaries[2] = connectPoints(corner3, corner4);
        limits.boundaries[3] = connectPoints(corner4, corner1);
        limits.centroid = new Vector2(0, 0); //does not have to be 0,0. can be centered elsewhere. change/ make this better when needed.



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
                Vector2 difference = currentPoint - point;//again working w/ lines while avoiding slope formula because fml
                Vector2 perpendicular = new Vector2(-difference.y, difference.x);  // this gives slope

                Vector2 midSegment1 = midpoint + (101 * perpendicular);//this is to have the bisecting line cross all boundaries, for intersection(Line a, Line b) checking purpoes.
                Vector2 midSegment2 = midpoint + (-101 * perpendicular);//100 is just the value im using rn change later. 101+will fs be outside voroni boundary but maybe not a good way 2 do this

                Line bisector = new Line();
                bisector.start = midSegment1;
                bisector.end = midSegment2;

                int intersections = 0;
                foreach (Line cellBoundary in cell.boundaries)
                {
                    if(intersects(bisector, cellBoundary))  //create a intersect function that returns interseciton as to actually use the intersection information. am i stupid this sounds simple
                    {
                        intersections++;
                    }
                }




            }
        }
    }

    private void makePoints()
    {
        for(int i = 0; i < centroids.Length; i++)
        {
            float randX = Random.Range(minX, maxX);
            float randZ = Random.Range(minZ, maxZ);

            centroids[i] = new Vector2(randX, randZ);
        }
    }

    
    private Line connectPoints(Vector2 a, Vector2 b)
    {
        Line newLine = new Line();
        newLine.start = a;
        newLine.end = b;

        return newLine;
    }


    private bool inCell(Vector2 point, Cell box)
    {
        Line innerLine = new Line();
        innerLine.start = box.centroid;
        //draw line from centroid to point. if that line intersects a Cell line then is not inside.    
        //      !!edge case exists where point is on the line     (could be problem, but will not show up in use case of creating voroni cell).
        foreach (Line line in box.boundaries)
        {
            if (intersects(innerLine, line))
            {
                return false;
            }
        }

        return true;
    }

    private bool intersects(Line a, Line b) //from geeksforgeeks.org. Intersections but also avoiding diviision by zero
    {
        float a1 = a.start.y - a.end.y;
        float b1 = a.start.x - a.end.x;
        float line1 = a1 * a.start.x + b1 * a.end.y;

        float a2 = b.start.y - b.end.y;
        float b2 = b.start.x - b.end.x;
        float line2 = a2 * b.start.x + b2 * b.end.y;

        //parallel or collinear lines, by deteriminant
        float det = a1 * b2 - a2 * b1;

        if (det == 0)
        {
            //parallel but different
            if (a1 * b.start.x + b1 * b.start.y != line1)
            {
                return false;
            }

            //collinear
            bool noOverlap = !(Mathf.Max(a.start.x, a.end.x) < Mathf.Min(b.start.x, b.end.x) //line a's greatest X value is less than b's smallest X value
                || Mathf.Max(b.start.x, b.end.x) < Mathf.Min(a.start.x, a.end.x) // or line b's greatest X value is less than a's smallest X value 
                || Mathf.Max(b.start.x, b.end.x) < Mathf.Min(a.start.x, a.end.x)
                || Mathf.Max(a.start.y, a.end.y) < Mathf.Min(b.start.y, b.end.y) // same for y values, if there is no overlap at least one of these will be true, meaning no intersect for collinear lines.
                || Mathf.Max(b.start.y, b.end.y) < Mathf.Min(a.start.y, a.end.y));

            return noOverlap;
        }

        float xNum = b2 * line1 - b1 * line2;
        float yNum = a1 * line2 - a2 * line1; // intersect x, y points

        //bounding boxes
        float minX1 = Mathf.Min(a.end.x, a.start.x);
        float maxX1 = Mathf.Max(a.end.x, a.start.x);
        float minY1 = Mathf.Min(a.end.y, a.start.y);
        float maxY1 = Mathf.Max(a.end.y, a.start.y);

        float minX2 = Mathf.Min(b.end.x, b.start.x);
        float maxX2 = Mathf.Max(b.end.x, b.start.x);
        float minY2 = Mathf.Min(b.end.y, b.start.y);
        float maxY2 = Mathf.Max(b.end.y, b.start.y);

        // Make the denominator positive so that
        // all comparisons below use the same direction.
        if (det < 0)
        {
            det = -det;
            xNum = -xNum;
            yNum = -yNum;
        }

        // Check whether the intersection point lies
        // inside both line segments.
        return xNum >= minX1 * det && xNum <= maxX1 * det
            && yNum >= minY1 * det && yNum <= maxY1 * det
            && xNum >= minX2 * det && xNum <= maxX2 * det
            && yNum >= minY2 * det && yNum <= maxY2 * det;
    }
}
