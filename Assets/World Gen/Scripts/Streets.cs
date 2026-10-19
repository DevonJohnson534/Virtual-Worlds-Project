using System.IO;
using System.Net;
using UnityEngine;
using Voronoi;

public class Streets : MonoBehaviour
{
	public float thickness = 2;
	public Material streetMaterial;
	private CellHandler cellHandler;

    private void Start()
    {
		cellHandler = FindAnyObjectByType<CellHandler>();
        buildStreets(cellHandler.cells, thickness);
    }

    private void buildStreets(Cell[] cells, float thickness)
	{
		foreach(Cell cell in cells)
		{
			foreach(Line boundary in cell.boundaries)
			{
			Vector2[] topVerticies = new Vector2[4];
			topVerticies[0] = boundary.start;
			topVerticies[1] = boundary.end;
			topVerticies[2] = getInnerPoint(boundary.start, cell.center, thickness);
			topVerticies[3] = getInnerPoint(boundary.end, cell.center, thickness);

			GameObject go = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
			go.transform.localScale = new Vector3(1, 1, 1);
			go.GetComponent<MeshFilter>().mesh = getMesh(topVerticies);
			go.GetComponent<MeshRenderer>().material = streetMaterial;
            }
		}
	}

	private static Vector2 getInnerPoint(Vector2 outerPoint, Vector2 centerPoint, float distance)
	{
		Vector2 direction = outerPoint - centerPoint;
		return direction.normalized * distance;
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

        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;

		Mesh mesh = new Mesh();

        mesh.vertices = worldVerticies;
        mesh.triangles = triangles;
        mesh.uv = topVerticies;
		mesh.RecalculateNormals();
		return mesh;
    }



	public static void materializeCells(Cell[] cells) // visualization purposes. 
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
			structure.transform.position = new Vector3(c.center.x, 1, c.center.y);
			structure.name = "center" + count;

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