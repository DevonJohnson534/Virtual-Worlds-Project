using BuildingGen;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class demoBuilder : MonoBehaviour
{
    public GameObject[] prefabs;

    void Start()
    {
        CellHandler cellHandler = FindAnyObjectByType<CellHandler>();
        List <Voronoi.Cell> cells = cellHandler.cells;

        foreach (Voronoi.Cell c in cells)
        {
            Vector3 origin = new Vector3(c.getCenter().x, 1, c.getCenter().y);

            int x = Random.Range(4, 2);
            int y = Random.Range(3, 20);
            int z = Random.Range(x - 1, x + 1);


            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
            GameObject instantiated = Instantiate(prefabs[(int) Random.Range(0,prefabs.Length)], origin, rotation);
            Vector3Int dimensions = new Vector3Int(x, y, z);
            instantiated.GetComponent<BuildingGenerator>().StartGeneration(dimensions);


        }


    }


}
