using Mono.Cecil.Cil;
using UnityEngine;
namespace BuildingGen
{
   public class TestBuildingInstantiate : MonoBehaviour
   {
      public BuildingGenerator building;
      void Start()
      {
         Vector3Int dimensions = new Vector3Int(4,5,6);

			GameObject instantiated = Instantiate(building.gameObject, this.transform);
			instantiated.GetComponent<BuildingGenerator>().StartGeneration(dimensions);
		}
   }
}