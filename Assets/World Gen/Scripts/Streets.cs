using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using Voronoi;
using System.Collections.Generic;


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

		foreach(Cell cell in cells)
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
				go.name = "wall of point: " + count;
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
        Color[] colors = new UnityEngine.Color[3];
		colors[0] = Color.red;
		colors[1] = Color.green;
		colors[2] = Color.blue;
		int count = 0;

		foreach (Cell c in cells)
		{
			GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);// make ovbious
			structure.transform.localScale = new Vector3(2, 1, 2);
			structure.transform.position = new Vector3(c.getPoint().x, 1, c.getPoint().y);
			structure.name = "point" + count;

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
				wallcount++;
			}
			count++;
		}
	}
 }