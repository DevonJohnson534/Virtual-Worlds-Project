using UnityEngine;

namespace BuildingGen
{
	[System.Serializable]
	public abstract class EdgeInfo
	{
		public int edgeName;
		public bool symmetric;

		public override string ToString()
		{
			return edgeName.ToString(); //TODO include everything
		}
	}
	[System.Serializable]
	public class HorzEdge : EdgeInfo
	{
		public bool flipped; //for non-symmetric faces only
	}
	[System.Serializable]
	public class VertEdge : EdgeInfo
	{
		public int rotIndex; //for non-symmetric faces only. rotations about the y axis, can be 0,1,2,3.
	}
}