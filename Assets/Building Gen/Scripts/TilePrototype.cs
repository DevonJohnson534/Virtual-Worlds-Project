using UnityEngine;

namespace BuildingGen
{
	public class TilePrototype : MonoBehaviour
	{
		[Header("Edge order: " +
			"\n0 = X_NEG" +
			"\n1 = Y_NEG" +
			"\n2 = Z_NEG" +
			"\n3 = X_POS" +
			"\n4 = Y_POS" +
			"\n5 = Z_POS")]
		public EdgeInfo[] edges = new EdgeInfo[6];
	}
}