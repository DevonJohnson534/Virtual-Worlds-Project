using System.Collections.Generic;
using UnityEngine;
using Voronoi;

public class BiomeHandler
{
    //uncomment this to run!!
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void biomeCreation()
    {
        float boundDistance = 2500; ;

        Vector2 outerBounds = new Vector2(boundDistance, boundDistance);
        List<Vector2> centerPoints = RandomPoints.getRandomPoints(outerBounds, 250, 500);
        VoronoiDiagram voronoi = new VoronoiDiagram(boundDistance);
        Cell[] cells = voronoi.getCells(centerPoints);
        Streets.buildStreets(cells);

    }
}
