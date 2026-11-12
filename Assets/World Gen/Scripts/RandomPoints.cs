using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/*
 * RandomPoints.getRandomPoints(a, b, c);
 * A: Vector representing bounds biomes can be within.
 * B: number of distinct areas.
 * C: minimum distance between biomes.
 * As A Decreases: less likely a random point is within bounds -> more expensive to calculate new points.
 * As C increases: less likely a random point is within bounds && not in another's space -> more expensive to calculate new points.
 * As B increases: more points to find their place.
 * A ratio I think works okay is 10A:1B:2C
 */
public static class RandomPoints //Poission disk sampling Sebastian Lague on youtube
{
    public static List<Vector2> getRandomPoints(Vector2 limits, int numPoints, float minDistance)
    {
        float gridUnit = minDistance / Mathf.Sqrt(2);
        int[,] grid = new int[(int) Mathf.Ceil(limits.x / gridUnit) + 1, (int) Mathf.Ceil(limits.y / gridUnit) + 1];
        List<Vector2> allPoints = new List<Vector2>();

        List<Vector2> branchPoints = new List<Vector2>();

        //0,0 always has a cell 'centered' around it.
        branchPoints.Add(limits / 2);
        allPoints.Add(Vector2.zero);
        grid[0, 0] = 1;


        while (branchPoints.Count > 0 && allPoints.Count <= numPoints)
        {
            int randIndex = Random.Range(0, branchPoints.Count);
            Vector2 branchPoint = branchPoints[randIndex];

            bool created = false;

            for(int i = 0; i < 8; i++)
            {
                float angle = Random.Range(0, 2 * Mathf.PI);
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidatePoint = branchPoint + (direction * Random.Range(minDistance, 4 * minDistance)); //i think im technically wrong in multiplying by 4 and not 2 but I like the look of this.

                int gridX = (int) Mathf.Ceil(candidatePoint.x / gridUnit); 
                int gridY = (int) Mathf.Ceil(candidatePoint.y / gridUnit);

                if(validPoint(candidatePoint, grid, gridUnit, allPoints, limits, minDistance))
                {
                    allPoints.Add((2 * candidatePoint) - limits);
                    branchPoints.Add(candidatePoint);
                    grid[gridX, gridY] = allPoints.Count;

                    created = true;
                    break;
                }
            }

            if (!created)
            {
                branchPoints.RemoveAt(randIndex);
            }

        }

        return allPoints;
    }


    public static List<Vector2> getPredefinedPoints() // i use this for testing
    {
        List<Vector2> allPoints = new List<Vector2>();
        allPoints.Add(Vector2.zero);
        allPoints.Add(new Vector2(-69.66785f, 74.59938f));

        return allPoints;
    }

    private static bool validPoint(Vector2 point, int[,] grid, float gridUnit, List<Vector2> allPoints, Vector2 limits, float minDistance)
    {
        if (point.x < 0 || point.x > limits.x)
        {
            return false;
        }

        if (point.y < 0 || point.y > limits.y)
        {
            return false;
        }

        int gridX = (int)Mathf.Ceil(point.x / gridUnit);
        int gridY = (int)Mathf.Ceil(point.y / gridUnit);

        int minX = Mathf.Max(gridX - 2, 0);
        int maxX = Mathf.Min(gridX + 2, grid.GetLength(0) - 1);

        int minY = Mathf.Max(gridY - 2, 0);
        int maxY = Mathf.Min(gridY + 2, grid.GetLength(1) - 1);

        for (int i = minX; i < maxX + 1; i++)
        {
            for (int j = minY; j < maxY + 1; j++)
            {
                int index = grid[i, j] - 1;
                if(index != -1)
                {
                    float sqrdistance = (point - allPoints[index]).sqrMagnitude;

                    if (sqrdistance < minDistance * minDistance)
                    {
                        return false;
                    }
                }
            }
        }


        return true;
    }

}