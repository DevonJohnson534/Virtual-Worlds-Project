using BuildingGen;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Voronoi;

public class CellHandler : MonoBehaviour
{
    public float boundDistance;
    public int numberOfCells;
    public float minDistance;
    public float StreetThickness = 4f;

    public Material streetMaterial;
    public GameObject[] prefabs;

    public bool cellsReady = false;
    public List<Voronoi.Cell> cells;
    private List<Voronoi.Cell> streetCells;

    void Awake()
    {
        Vector2 outerBounds = new Vector2(boundDistance, boundDistance);
        List<Vector2> Points = RandomPoints.getRandomPoints(outerBounds, numberOfCells, minDistance);
        VoronoiDiagram voronoi = new VoronoiDiagram(boundDistance);
        cells = voronoi.getCells(Points);

        Streets streets = new Streets(StreetThickness, streetMaterial);
        streetCells = streets.buildStreets(cells);
        cellsReady = true;
    }


    private void Start()
    {
        int limit = (int) Mathf.Min(streetCells.Count, 1);

        for(int i = 0; i < limit; i++)
        {
            populateCell(streetCells[i]);
        }

    }

    private void populateCell(Voronoi.Cell c)
    {

        List<Voronoi.Cell> plots = CellBuildings.getConstructionZones(c);

        foreach (Voronoi.Cell plot in plots)
        {
            
            Streets.materializeCell(plot, -1);
            /*
            Vector3 origin = new Vector3(plot.getCenter().x, 1, plot.getCenter().y);

            int x = Mathf.FloorToInt((plot.boundaries[0].getLength() - 2) / 2);
            int y = Random.Range(10, 20);
            int z = Mathf.FloorToInt((plot.boundaries[1].getLength() - 2) / 2);

            float rotationY = 180 - Mathf.Rad2Deg * Mathf.Atan2(plot.boundaries[0].getDirection().y, plot.boundaries[0].getDirection().x);


            Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);

            GameObject instantiated = Instantiate(prefabs[(int)Random.Range(0, prefabs.Length)], origin, rotation);
            Vector3Int dimensions = new Vector3Int(x, y, z);
            instantiated.GetComponent<BuildingGenerator>().StartGeneration(dimensions);*/
        }
    }
}
