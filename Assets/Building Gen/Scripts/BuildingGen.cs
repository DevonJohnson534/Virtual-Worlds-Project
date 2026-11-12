using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BuildingGen
{
	public class BuildingGenerator : MonoBehaviour
	{
		public Vector3Int dimensions;
		public Vector3 tileSize;
		public TilePrototype[] tilePrototypes;

		public TilePrototype magicTilePrototype;  //the magic tile can be next to anything b/c all -1 edges
		//public TilePrototype airTilePrototype;    //used as the boundary for the sides and top
		//public TilePrototype groundTilePrototype; //used as the boundary for bottom

		private Tile magicTile;
		//private Tile airTile;
		//private Tile groundTile;

		private List<Tile> tileSet;
		private Cell[,,] cells; //x,y,z

		public void StartGeneration(Vector3Int _dimensions)
		{
			dimensions = _dimensions;
			Setup();
			InitCells();
			//CreateBoundaries();

			bool generating = true;
			while (generating)
			{
				List<Vector3Int> collapseCandidates = FindCollapseCandidates();

				if (collapseCandidates == null)
				{
					generating = false;
					Cleanup();
					Debug.Log("Generation complete");
				}
				else
				{
					Vector3Int cords = CollapseCell(collapseCandidates);
					PropagateConstraints(cords);
				}
			}
		}

		private void Setup() //in the future this should maybe be done once and stored, not at runtime. or at least shared between buildings of the same type
		{
			//Generate rotated versions of all tile prototypes
			tileSet = new List<Tile>();
			foreach (TilePrototype p in tilePrototypes)
			{
				for (int i = 0; i < 4; i++)
				{
					tileSet.Add(new Tile(i, p));
				}
			}

			//See which tiles have matching sockets, add to neighbor_list
			for (int x = 0; x < tileSet.Count; x++)
			{
				for (int y = 0; y < tileSet.Count; y++)
				{
					if (y >= x) //don't double up
					{
						tileSet[y].ConsiderNeighboring(tileSet[x]);
					}
				}
			}

			//Create the magic tile and generate its neighbor rules. same for air and ground tiles
			magicTile = SetupSpecialTile(magicTilePrototype, new Tile[0]);
			//airTile = SetupSpecialTile(airTilePrototype, new Tile[] {magicTile});
			//groundTile = SetupSpecialTile(groundTilePrototype, new Tile[] { magicTile, airTile});
		}
		private Tile SetupSpecialTile(TilePrototype proto, Tile[] otherSpecials)
		{
			Tile specialTile = (new Tile(0, proto));
			foreach (Tile t in tileSet.Union(otherSpecials))
			{
				t.ConsiderNeighboring(specialTile);
			}
			specialTile.ConsiderNeighboring(specialTile);
			return specialTile;
		}

		private void Cleanup()
		{
			//Removes some things that aren't needed anymore after generation.
			cells = null;
			tileSet = null;
		}

		private void InitCells()
		{
			//Create cells at every grid location.
			dimensions = dimensions + new Vector3Int(2, 2, 2); //make dimensions bigger to accomidate boundary "cells"
			cells = new Cell[dimensions.x, dimensions.y, dimensions.z];

			for (int x = 0; x < dimensions.x; x++)
			{
				for (int y = 0; y < dimensions.y; y++)
				{
					for (int z = 0; z < dimensions.z; z++)
					{
						Cell newCell = new Cell();
						cells[x, y, z] = newCell;
						newCell.InitCell(false, tileSet);

					}
				}
			}
		}
		//private void CreateBoundaries()
		//{
		//	//Create boundaries, so the outside is air and the bottom is ground. these don't actually get instantiated
		//	//This could be a bit faster without the whole loop but its fine
		//	for (int x = 0; x < dimensions.x; x++)
		//	{
		//		for (int y = 0; y < dimensions.y; y++)
		//		{
		//			for (int z = 0; z < dimensions.z; z++)
		//			{
		//				//on the top or sides, not a real cell and used for boundary constraint
		//				if (x == 0 || z == 0 || x == dimensions.x - 1 || y == dimensions.y - 1 || z == dimensions.z - 1)
		//				{
		//					cells[x, y, z].collapsed = true;
		//					cells[x, y, z].possibleTiles = new() { airTile };
		//					PropagateConstraints(new Vector3Int(x, y, z));

		//					////TEMP DEBUG
		//					//TilePrototype instantiated = Instantiate(airTile.prototype, this.transform); //instantiated tiles are children of this building
		//					//instantiated.transform.localPosition = new Vector3(x * tileSize.x, y * tileSize.y, z * tileSize.z); //TODO -1,-1,-1?
		//					//instantiated.transform.localRotation = airTile.getRotation();

		//				}
		//				//on the bottom, also not a real cell and used for boundary constraint
		//				else if (y == 0)
		//				{
		//					cells[x, y, z].collapsed = true;
		//					cells[x, y, z].possibleTiles = new() { groundTile };
		//					PropagateConstraints(new Vector3Int(x, y, z));

		//					////TEMP DEBUG
		//					//TilePrototype instantiated = Instantiate(groundTile.prototype, this.transform); //instantiated tiles are children of this building
		//					//instantiated.transform.localPosition = new Vector3(x * tileSize.x, y * tileSize.y, z * tileSize.z); //TODO -1,-1,-1?
		//					//instantiated.transform.localRotation = groundTile.getRotation();
		//				}
		//			}
		//		}
		//	}
		//}

		private List<Vector3Int> FindCollapseCandidates() //TODO this could be a lot faster
		{
			//Return a list of the lowest entropy tiles. If everything is collapsed, return null.

			int currentE = int.MaxValue;
			List<Vector3Int> currentCords = null;

			for (int x = 0; x < dimensions.x; x++)
			{
				for (int y = 0; y < dimensions.y; y++)
				{
					for (int z = 0; z < dimensions.z; z++)
					{
						Cell c = cells[x, y, z];

						if (c.collapsed == true) continue; //skip collapsed cells
						else if (c.possibleTiles.Count == 0) //avoid impossible collapses by adding the magic tile
						{
							Debug.Log("possible tiles is zero, but not collapsed. adding magic tile.");
							c.possibleTiles.Add(magicTile);
						}

						if (c.possibleTiles.Count < currentE) //new lowest entropy found
						{
							currentE = c.possibleTiles.Count;
							currentCords = new List<Vector3Int> { new Vector3Int(x, y, z) };
						}
						else if (c.possibleTiles.Count == currentE)
						{
							currentCords.Add(new Vector3Int(x, y, z));
						}
					}
				}
			}

			return currentCords;
		}
		private Vector3Int CollapseCell(List<Vector3Int> candidates)
		{
			//Collapses a random candiate cell to to a single tile and instantiates that tile.
			Vector3Int cords = candidates[UnityEngine.Random.Range(0, candidates.Count)];

			Cell collapsingCell = cells[cords.x, cords.y, cords.z];
			collapsingCell.collapsed = true;

			int TileIndex = UnityEngine.Random.Range(0, collapsingCell.possibleTiles.Count);
			Tile collapsedTile = collapsingCell.possibleTiles[TileIndex];
			collapsingCell.possibleTiles = new List<Tile> { collapsedTile };

			TilePrototype instantiated = Instantiate(collapsedTile.prototype, this.transform); //instantiated tiles are children of this building
			instantiated.transform.localPosition = new Vector3(cords.x * tileSize.x, cords.y * tileSize.y, cords.z * tileSize.z); //TODO -1,-1,-1?
			instantiated.transform.localRotation = collapsedTile.getRotation();

			return cords;
		}

		private void PropagateConstraints(Vector3Int firstCords)
		{
			//Starting with the neighbors of "firstCords", remove invalid possible tiles from cells.

			Queue<Vector3Int> cordsToProcess = new Queue<Vector3Int>();
			cordsToProcess.Enqueue(firstCords);

			while (cordsToProcess.Count > 0)
			{
				Vector3Int cords = cordsToProcess.Dequeue();

				List<Vector3Int> changedCords = UpdateNeighbors(cords);
				foreach (Vector3Int n in changedCords)
				{
					if (!cordsToProcess.Contains(n)) cordsToProcess.Enqueue(n);
				}
			}
		}
		private List<Vector3Int> UpdateNeighbors(Vector3Int cords)
		{
			// For every cell surrounding "cords":
			//		Check the cell in each direction. remove any elements of its possibleTiles that are no longer viable.
			//    For every cell that got updated, return its neighbors so they can also get checked

			List<Tile> possibleAtCords = cells[cords.x, cords.y, cords.z].possibleTiles;
			if (possibleAtCords.Count == 0) return new List<Vector3Int>(); // don't update any neighbors because theres a magic tile

			Vector3Int[] neighborDirs = { new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
												new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
												new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)};
			HashSet<Tile>[] validNeighborsOrdered = new HashSet<Tile>[6] { new(), new(), new(), new(), new(), new() }; //TODO improve syntax...
			foreach (Tile t in possibleAtCords)
			{
				validNeighborsOrdered[0].UnionWith(t.neighbors[Directions.X_POS]);
				validNeighborsOrdered[1].UnionWith(t.neighbors[Directions.X_NEG]);
				validNeighborsOrdered[2].UnionWith(t.neighbors[Directions.Y_POS]);
				validNeighborsOrdered[3].UnionWith(t.neighbors[Directions.Y_NEG]);
				validNeighborsOrdered[4].UnionWith(t.neighbors[Directions.Z_POS]);
				validNeighborsOrdered[5].UnionWith(t.neighbors[Directions.Z_NEG]);
			}

			List<Vector3Int> impactedCords = new List<Vector3Int>();
			for (int i = 0; i < neighborDirs.Length; i++)
			{
				Vector3Int nCords = neighborDirs[i] + cords;
				if (!InBounds(nCords)) continue;

				Cell neighbor = cells[nCords.x, nCords.y, nCords.z];
				if (neighbor.collapsed) continue;

				bool listChanged = EnsureValidity(neighbor.possibleTiles, validNeighborsOrdered[i]);
				if (listChanged) impactedCords.Add(nCords);
			}
			return impactedCords;
		}

		private bool InBounds(Vector3Int cords)
		{
			return (cords.x < dimensions.x & cords.y < dimensions.y & cords.z < dimensions.z
					& cords.x > -1 & cords.y > -1 & cords.z > -1);
		}

		private bool EnsureValidity(List<Tile> optionList, HashSet<Tile> validOptions)
		{
			// If a tile can't be next to another tile, remove it from the list of possible tiles
			bool listEdited = false;

			for (int x = optionList.Count - 1; x >= 0; x--)
			{
				var element = optionList[x];

				if (!validOptions.Contains(element))
				{
					optionList.RemoveAt(x);
					listEdited = true;
				}
			}

			return listEdited;
		}

	}
}