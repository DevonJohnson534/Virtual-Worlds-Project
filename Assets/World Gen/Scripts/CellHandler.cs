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
        foreach(Voronoi.Cell c in streetCells)
        {
            populateCell(c);
        }

    }

    private void populateCell(Voronoi.Cell c)
    {

        Line[] plot = CellBuildings.constructionZone(c);

        Voronoi.Cell rectangle = new Voronoi.Cell(plot, c.getPoint());
        Streets.materializeCell(rectangle, -1);  

        Vector3 origin = new Vector3(plot[0].start.x, 1, plot[0].start.y);

        int x = Mathf.FloorToInt((plot[0].getLength()-2) / 2);
        int y = Random.Range(10, 20);
        int z = Mathf.FloorToInt((plot[1].getLength()-2) / 2);

        float rotationY = Mathf.Rad2Deg * Mathf.Atan2(plot[0].getDirection().x, plot[0].getDirection().y); // im a  chud


        Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);

        GameObject instantiated = Instantiate(prefabs[(int)Random.Range(0, prefabs.Length)], origin, rotation);
        Vector3Int dimensions = new Vector3Int(x, y, z);
        instantiated.GetComponent<BuildingGenerator>().StartGeneration(dimensions);

    }
}
