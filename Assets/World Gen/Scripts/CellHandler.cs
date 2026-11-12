using System.Collections.Generic;
using UnityEngine;
using Voronoi;

public class CellHandler : MonoBehaviour
{
    public float boundDistance;
    public int numberOfCells;
    public float minDistance;
    public float StreetThickness = 4f;
    public Material streetMaterial;

    public List<Cell> cells;

    void Awake()
    {
        Vector2 outerBounds = new Vector2(boundDistance, boundDistance);
        List<Vector2> Points = RandomPoints.getRandomPoints(outerBounds, numberOfCells, minDistance);
        VoronoiDiagram voronoi = new VoronoiDiagram(boundDistance);
        cells = voronoi.getCells(Points);

        Streets streets = new Streets(StreetThickness, streetMaterial);
        List<Cell> streetCells = streets.buildStreets(cells);
        //BuildingPlacement.plotBuildings(streetCells);

    }
}
