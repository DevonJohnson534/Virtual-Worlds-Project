using UnityEngine;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using Voronoi;
using System.Collections.Generic;

public class BuildingPlacement
{
	public static void plotBuildings(List<Cell> cells)
	{
		foreach(Cell cell in cells)
		{	
			Cell rectangle = getSafeBuildingArea(cell);
			List<Cell> a = new List<Cell>();
			a.Add(rectangle);
			Streets.materializeCells(a);
			Debug.Log("what");

		}
	}


	public static Cell getSafeBuildingArea(Cell cell)
	{
		Line[] boundaries = cell.boundaries;
		
		int longIndex = cell.longestBorder();
		Line longest = boundaries[longIndex];
		Line[] candidateEdges = newBuildingLimitEdges(longest);
		Vector2[] corners = new Vector2[4];


		Vector2? intersection1 = null;
		Vector2? intersection2 = null;
		int intersectedIndex1 = -1; 
		int intersectedIndex2 = -1; 

		
		for(int i = longIndex + 2; i < longIndex + boundaries.Length - 1; i++)
		{
			int index = i % boundaries.Length;

			intersection1 = candidateEdges[0].intersection(boundaries[index]);
			intersection2 = candidateEdges[1].intersection(boundaries[index]);

			if(intersection1.HasValue && intersectedIndex1 == -1)
			{
				intersectedIndex1 = index;
			}			
			
			if(intersection2.HasValue && intersectedIndex2 == -1)
			{
				intersectedIndex2 = index;
			}


		}

		if(intersectedIndex1 + intersectedIndex2 == -2)
		{
			return acuteEdgeCase(boundaries, longIndex);
		}

		else if(intersection1.HasValue && intersection2.HasValue)
		{
			Vector2[] rectangleCorners = new Vector2[4];
			rectangleCorners[0] = longest.start;	
			rectangleCorners[1] = longest.end;
			rectangleCorners[2] = intersection2.Value;
			rectangleCorners[3] = intersection1.Value;

			return new Cell(rectangleCorners);
		}

		else if(intersection1.HasValue && !intersection2.HasValue)
		{
			Vector2[] rectangleCorners = new Vector2[4];

			rectangleCorners[0] = longest.start;
			rectangleCorners[1] = boundaries[(longIndex + 1) % boundaries.Length].end - (2 * longest.getDirection() * longest.getLength());
			rectangleCorners[2] = boundaries[(longIndex + 1) % boundaries.Length].end;
			rectangleCorners[3] = new Vector2(rectangleCorners[0].y, rectangleCorners[0].x);
		}

		return null;
	}


	private static Vector2? adjacentCorner(Line incoming, Line[] boundaries, int lastIndex, bool clockwise, float safeDist)
	{		
		Vector2 dir = incoming.getDirection();
		int pdir = clockwise? 1 : -1;
		Vector2 perpendicular = new Vector2(pdir * dir.y, dir.x);

		Vector2 lastCorner = incoming.end;

		Line outgoing = new Line (lastCorner, lastCorner + perpendicular * safeDist);

		for(int i = lastIndex + 1; i < boundaries.Length + lastIndex - 1; i++)
		{
			int index = i % boundaries.Length;

			Vector2? nextIntersection = outgoing.intersection(boundaries[index]);

			if (nextIntersection.HasValue)
			{
				return nextIntersection;
			}
		}

		return null;
	}


	private static Line[] newBuildingLimitEdges(Line firstEdge)
	{
		Vector2 direction = firstEdge.getDirection(); //points from end to start 
    	Vector2 perpendicular = new Vector2(direction.y, direction.x); // if direction points up, this points to the right. our lines all 'point' clockwise so this always work ong.

		Line[] candidateEdges = new Line[2];
		candidateEdges[0] = new Line(firstEdge.start, firstEdge.start + (perpendicular * 2 * firstEdge.getLength()));
		candidateEdges[1] = new Line(firstEdge.end, firstEdge.end + (perpendicular * 2 * firstEdge.getLength()));

		return candidateEdges;	
	}


	private static Cell acuteEdgeCase(Line[] boundaries, int longestIndex)
	{	
		Line longest = boundaries[longestIndex];

		Vector2 direction = longest.getDirection(); 
    	Vector2 perpendicular = new Vector2(direction.y, direction.x);

		float lengthFraction = longest.getLength() / 3;

		Vector2[] corners = new Vector2[4];

		corners[0] = longest.start + (direction * lengthFraction);
		corners[1] = longest.end - (direction * lengthFraction);

		Line fromStart = new Line(corners[0], corners[0]  + (perpendicular * 2 * longest.getLength()));	
		Line fromEnd = new Line(corners[1], corners[1]  + (perpendicular * 2 * longest.getLength()));

		fromStart.end = shortenToIntersection(boundaries, longestIndex, fromStart).Value;
		fromEnd.end = shortenToIntersection(boundaries, longestIndex, fromEnd).Value;



        if (fromStart.getLength() <= fromEnd.getLength())
		{
			corners[3] = fromStart.end;
			corners[2] = fromEnd.intersection(new Line(corners[3], corners[3] + (direction * longest.getLength()))).Value;
		}
        else if (fromStart.getLength() < fromEnd.getLength())
        {
            corners[2] = fromEnd.end;
            corners[3] = fromStart.intersection(new Line(corners[2], corners[2] + (direction * longest.getLength()))).Value;
        }


        foreach (Vector2 v in corners)
		{
			Debug.Log(v);
		}

		return new Cell(corners);
	}


	private static Vector2? shortenToIntersection(Line[] boundaries, int skip, Line line)
	{
        for (int i = skip + 1; i < boundaries.Length + skip; i++)
        {
            int index = i % boundaries.Length;

            Vector2? intersection = line.intersection(boundaries[index]);

            if (intersection.HasValue)
            {
                return intersection;
            }
        }
		return null;
    }


	private float cosineSimilarity(Vector2 u, Vector2 v)
	{
		float magnitudes = u.magnitude * v.magnitude;
		return Vector2.Dot(u, v) / magnitudes;
	}


	private Vector2 quadrant(Vector2 u)
	{
		Vector2 nearestAxis = Vector2.zero;

		float absX = Mathf.Abs(u.x);
		float absY = Mathf.Abs(u.y);

		if(absX >= absY) //absX=absY is an edge case that scares me and I will not think about it
		{
			return new Vector2((u.x) / absX, 0);

		}		

		if(absY > absX)
		{
			return new Vector2(0, (u.y) / absY);
		}

		return nearestAxis;
	}

}
