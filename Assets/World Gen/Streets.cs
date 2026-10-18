using UnityEngine;

namespace Voronoi
{
    public static class Streets
    {
        public static void buildStreets(Cell[] cells)
        {
            Color[] colors = new Color[3];
            colors[0] = Color.red;
            colors[1] = Color.green;
            colors[2] = Color.blue;
            int count = 0;

            foreach (Cell c in cells)
            {
                GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);// make ovbious
                structure.transform.localScale = new Vector3(2, 1, 2);
                structure.transform.position = new Vector3(c.centroid.x, 1, c.centroid.y);
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
}