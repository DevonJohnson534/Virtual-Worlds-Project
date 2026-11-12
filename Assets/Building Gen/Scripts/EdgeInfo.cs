
namespace BuildingGen
{
	[System.Serializable]
	public class EdgeInfo
	{
		public int edgeName;
		public int rot;
		//-1 means symmetric, for both horizontal and vertical edges.
		//for horizontal edges, 0 is nonflipped and 1 is flipped
		//for vertical edges, 0,1,2,3 are all the rotational variants
		public EdgeInfo(int name, int rotationIndex)
		{
			edgeName = name;
			rot = rotationIndex;

		}
		public override string ToString()
		{
			return edgeName.ToString() +"_"+ rot.ToString();
		}
		public EdgeInfo CreateCopy()
		{
			return new EdgeInfo(edgeName,rot);
		}
	}
}