using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Voronoi;


public class Streets
{
	public List<Cell> streetCells;
	public Material streetMaterial;
	public float thickness = 4f;

	public Streets(float thickness, Material streetMaterial)
	{
		this.thickness = thickness / 2;
		this.streetMaterial = streetMaterial;
	}

    public List<Cell> buildStreets(List<Cell> cells)
	{
		int count = 0;

		List<Cell> inCells = new List<Cell>(cells.Count);
        GameObject parent = new GameObject("AllStreets");

        foreach (Cell cell in cells)
		{
			Line[] boundaries = cell.boundaries;
			Line[] inBoundaries = new Line[boundaries.Length];

			for (int i = 0; i < boundaries.Length; i++)
			{
				Line boundary = boundaries[i];

				Vector2[] topVerticies = new Vector2[4];
				topVerticies[0] = boundary.start;
				topVerticies[1] = boundary.end;
				topVerticies[2] = getBetweenPoint(boundary, boundaries[(i + 1) % boundaries.Length], thickness);
				topVerticies[3] = getBetweenPoint(boundaries[(boundaries.Length + (i - 1)) % boundaries.Length], boundary, thickness);

				inBoundaries[i] = new Line(topVerticies[3], topVerticies[2]);

				GameObject go = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
				
				go.transform.localScale = new Vector3(1, 1, 1);
				Mesh mesh = getMesh(topVerticies);
				go.GetComponent<MeshFilter>().mesh = mesh;
				go.GetComponent<MeshRenderer>().material = streetMaterial;
				go.GetComponent<MeshCollider>().sharedMesh = mesh;
				go.transform.parent = parent.transform;

                go.name = "street of point: " + count;
            }

			inCells.Add(new Cell(inBoundaries, cell.getPoint()));
			count++;
		}

		return inCells;
	}

    private Vector2 getBetweenPoint(Line a, Line b, float distance)
	{
		Vector2 midDirection = a.angleBisector(b);
		return a.end + (midDirection * distance);
    }




	private static Mesh getMesh(Vector2[] topVerticies)
	{
        Vector3[] worldVerticies = new Vector3[topVerticies.Length];
        for (int i = 0; i < topVerticies.Length; i++)
        {
            worldVerticies[i] = new Vector3(topVerticies[i].x, 0, topVerticies[i].y);
        }
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

		Mesh mesh = new Mesh();

        mesh.vertices = worldVerticies;
        mesh.triangles = triangles;
        mesh.uv = topVerticies;
		mesh.RecalculateNormals();
		return mesh;
    }


	public static void materializeCells(List<Cell> cells) // visualization purposes. 
	{
		int count = 0;

		foreach (Cell c in cells)
		{
			materializeCell(c, count);
			count++;
		}
	}



	public static void materializeCell(Cell c, int count)
	{
        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
        point.transform.localScale = new Vector3(2, 1, 2);
        point.transform.position = new Vector3(c.getPoint().x, 1, c.getPoint().y);
        point.name = "point" + count;

        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cube);
        center.transform.localScale = new Vector3(2, 1, 2);
        center.transform.position = new Vector3(c.getCenter().x, 1, c.getCenter().y);
        center.name = "center" + count;


        Color color = Random.ColorHSV();
        Renderer objRendererp = point.GetComponent<Renderer>();
        Renderer objRendererc = center.GetComponent<Renderer>();

        if (objRendererp != null)
        {
            objRendererp.material.color = color;

        }

        if (objRendererc != null)
        {
            objRendererc.material.color = color;

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
            cube.name = "boundary" + wallcount + "of cell " + count;

            if (direction != Vector3.zero)
            {
                cube.transform.rotation = Quaternion.LookRotation(direction);
            }
            Renderer o = cube.GetComponent<Renderer>();

            if (o != null)
            {
                o.material.color = color;

            }
            wallcount++;
        }
    }
 }