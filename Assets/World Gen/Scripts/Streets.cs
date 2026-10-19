using System.IO;
using System.Net;
using System.Threading;
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
		materializeCells(cellHandler.cells);
        buildStreets(cellHandler.cells, thickness);
    }

    private void buildStreets(Cell[] cells, float thickness)
	{
		int count = 0;
		foreach(Cell cell in cells)
		{
			Line[] boundaries = cell.boundaries;

			for (int i = 0; i < boundaries.Length; i++)
			{
				Line boundary = boundaries[i];

				Vector2[] topVerticies = new Vector2[4];
				topVerticies[0] = boundary.start;
				topVerticies[1] = boundary.end;
				topVerticies[2] = getBetweenPoint(boundary, boundaries[(i + 1) % boundaries.Length], thickness);
				topVerticies[3] = getBetweenPoint(boundaries[(boundaries.Length + (i - 1)) % boundaries.Length], boundary, thickness);


				GameObject go = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
				go.transform.localScale = new Vector3(1, 1, 1);
				go.GetComponent<MeshFilter>().mesh = getMesh(topVerticies);
				go.GetComponent<MeshRenderer>().material = streetMaterial;
				go.name = "wall of center: " + count;
            }
			count++;
		}
	}


    private Vector2 getBetweenPoint(Line a, Line b, float distance)
	{
		Vector2 mid = a.end;
		Vector2 dir1 = (a.start - mid).normalized;
		Vector2 dir2 = (b.end - mid).normalized;
		Vector2 midDirection = (dir1 + dir2).normalized;

		return midDirection * distance;
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
        triangles[5] = 1;

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