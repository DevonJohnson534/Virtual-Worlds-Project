using System.Collections.Generic;
using UnityEngine;

namespace BuildingGen
{
	public class Tile
	{
		public TilePrototype prototype;
		public EdgeInfo[] edges; //the order is in Directions.cs
		public List<Tile>[] neighbors;

		private int yRotations;

		public Tile(int yRots, TilePrototype proto)
		{
			prototype = proto;
			yRotations = yRots;

			edges = Directions.RotateEdges(proto.edges, yRots);
			neighbors = new List<Tile>[6];
			for (int i = 0; i < 6; i++) neighbors[i] = new();
		}

		public void ConsiderNeighboring(Tile other)
		{
			//compare edges with another tile, and add to the neighbors list for both this and the other tile if possible
			for (int i = 0; i < 6; i++)
			{
				int j = Directions.OppositeDir(i);
				bool edgesMatch = DoEdgesMatch(edges[i], other.edges[j], Directions.IsHorizontal(i));
				if (edgesMatch)
				{
					if (!neighbors[i].Contains(other)) neighbors[i].Add(other);
					if (!other.neighbors[j].Contains(this)) other.neighbors[j].Add(this);
				}
			}
		}

		private bool DoEdgesMatch(EdgeInfo a, EdgeInfo b, bool isHorizontal)
		{
			bool magicEdge = a.edgeName == -1 || b.edgeName == -1;
			if (magicEdge) return true;

			bool namesMatch = (a.edgeName == b.edgeName);
			if (!namesMatch) return false;

			if (a.rot == -1)
			{
				Debug.Assert(b.rot == -1); //they should both be -1 here, otherwise something is very wrong
				return true;
			}

			if (isHorizontal) return (a.rot != b.rot);
			else return (a.rot == b.rot);
		}

		public Quaternion getRotation()
		{
			return Quaternion.Euler(new Vector3(0, yRotations * 90, 0));
		}

		public override string ToString()
		{
			return prototype.name + "_" + yRotations + "_tile";
		}
	}
}