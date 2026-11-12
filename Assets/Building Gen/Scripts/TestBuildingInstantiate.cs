using Mono.Cecil.Cil;
using UnityEngine;
namespace BuildingGen
{
   public class TestBuildingInstantiate : MonoBehaviour
   {
      public BuildingGenerator building;
      public Vector3Int dimensions;
		void Start()
      {
			GameObject instantiated = Instantiate(building.gameObject, this.transform);
			instantiated.GetComponent<BuildingGenerator>().StartGeneration(dimensions);
		}
   }
}