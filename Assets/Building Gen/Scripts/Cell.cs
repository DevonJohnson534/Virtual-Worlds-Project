using System.Collections.Generic;

namespace BuildingGen
{
   public class Cell
   {
      public bool collapsed;
      public List<Tile> possibleTiles;

      public void InitCell(bool isCollapsed, List<Tile> tileSet)
      {
         collapsed = isCollapsed;
         possibleTiles = new List<Tile>(tileSet); //shallow copy
      }
   }
}