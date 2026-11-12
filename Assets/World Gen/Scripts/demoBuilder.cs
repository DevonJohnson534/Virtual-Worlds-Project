using BuildingGen;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
            int y = Random.Range(10, 20);
            int z = Random.Range(x - 1, x + 1);


            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
            Instantiate(prefabs[(int) Random.Range(0,prefabs.Length)], origin, rotation);



        }


    }


}
