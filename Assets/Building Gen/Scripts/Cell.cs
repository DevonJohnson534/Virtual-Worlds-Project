using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
   public bool collapsed;
	[System.NonSerialized] public List<Tile> possibleTiles;

   public void InitCell(bool isCollapsed, List<Tile> tileSet)
   {
      collapsed = isCollapsed;
      possibleTiles = new List<Tile>(tileSet); //shallow copy
	}
}
