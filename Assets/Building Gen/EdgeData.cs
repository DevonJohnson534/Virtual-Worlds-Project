using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif
//TODO finish rotations. right now it seems to be doing some flipping with the UV stuff. keep or change?
//TODO how to handle vertical vs horizontal edges?

namespace BuildingGen {
#if UNITY_EDITOR
	[CreateAssetMenu(fileName = "EdgeData", menuName = "Scriptable Objects/EdgeData")]
	public class EdgeData : ScriptableObject
	{
		private const string TilePrototypesDirectory = "Assets/Building Gen/Prefabs/Tile prototypes";
		private const int edgeDist = 1; //this is the distance from the center where the edges are. aka half of tile size
		private const float comparisonTolerance = 0.0005f; //used when comparing floats

		public Dictionary<string, int> profileToEdgeName { get; private set; }
		private int nextEdgeName;
		public class Profile
		{
			//a "Profile" is all the verticies on an "Edge" which is one of a mesh's 6 boundary box faces
			public List<Vector2> verts;
			public Profile(List<Vector2> _verts)
			{
				verts = _verts;
			}
			public string GetKey() //this is for the profileToEdgeName dictionary keys
			{
				if (verts == null || verts.Count == 0) return "EMPTY";

				//round to tolerance and cut it to 4 decimal places. put in readable format
				return string.Join("\n", verts.Select(v =>
					 $"({Mathf.Round(v.x / comparisonTolerance) * comparisonTolerance:F4}," +
					 $"{Mathf.Round(v.y / comparisonTolerance) * comparisonTolerance:F4})"));
			}
		}
		public void GenerateData()
		{
			Debug.Log("Starting data generation");
			//labels all the tile prototype prefabs with their correct edge data
			profileToEdgeName = new();
			profileToEdgeName.Add(new Profile(new()).GetKey(), 0); //add the empty profile as edge name 0
			nextEdgeName = 1;

			string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] { TilePrototypesDirectory });
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				TilePrototype tileProto = asset.GetComponent<TilePrototype>();

				if (asset.GetComponent<MeshFilter>() != null)
				{
					Debug.Log("Processing " + asset.name);
					Mesh mesh = asset.GetComponent<MeshFilter>().sharedMesh;

					SetEdgeInfo(asset.name, tileProto, mesh);
				}
				else
				{
					Debug.Log("Processing "+asset.name + ", but it has no mesh filter. adding empty edges.");
					tileProto.xMinusEdge.edgeName = 0;
					tileProto.xPlusEdge.edgeName = 0;
					tileProto.yMinusEdge.edgeName = 0;
					tileProto.yPlusEdge.edgeName = 0;
					tileProto.zMinusEdge.edgeName = 0;
					tileProto.zPlusEdge.edgeName = 0;
				}

				EditorUtility.SetDirty(tileProto); //this means that changes made to the prefab's TilePrototype can be saved
			}

			//AssetDatabase.SaveAssets(); //saves the edited prefabs automatically
			Debug.Log("Data generation complete!");
		}

		private void SetEdgeInfo(string assetName, TilePrototype tileProto, Mesh mesh)
		{
			tileProto.xPlusEdge.edgeName = FindEdgeName(assetName, mesh, 0, 1);
			tileProto.xMinusEdge.edgeName = FindEdgeName(assetName, mesh, 0, -1);

			tileProto.yPlusEdge.edgeName = FindEdgeName(assetName, mesh, 1, 1);
			tileProto.yMinusEdge.edgeName = FindEdgeName(assetName, mesh, 1, -1);

			tileProto.zPlusEdge.edgeName = FindEdgeName(assetName, mesh, 2, 1);
			tileProto.zMinusEdge.edgeName = FindEdgeName(assetName, mesh, 2, -1);
		}
		private Dictionary<int, (int,int,int)> uvFromProfileAxis = new(){ {0, (2,1,1)}, {1, (2,0,-1)}, { 2, (0,1,-1) } }; //axis => u, v, udir

		private int FindEdgeName(string name, Mesh mesh, int profileAxis, int profileDir)
		{
			//profileAxis is 0,1,2 meaning x,y,z    profileDir is -1 or 1 meaning if it is the positive or negative side
			List<Vector3> allVerts = new();
			mesh.GetVertices(allVerts);

			int uAxis = uvFromProfileAxis[profileAxis].Item1;
			int vAxis = uvFromProfileAxis[profileAxis].Item2;

			int uDir = uvFromProfileAxis[profileAxis].Item3;
			if (profileDir == -1) { uDir = -uDir; }

			//get the profile and store it as 2D
			List<Vector2> profileVerts = new();
			for (int i = 0; i < allVerts.Count; i++)
			{
				Vector3 vert = allVerts[i];
				if (name == "Cube") { Debug.Log(vert.x + "," + vert.y + "," + vert.z); }
				if (math.abs(vert[profileAxis] - profileDir * edgeDist) < comparisonTolerance)
				{
					if (name == "Cube") { Debug.Log("    On edge!"); }
					float u = vert[uAxis] * uDir;
					float v = vert[vAxis];

					profileVerts.Add(new Vector2(u, v));
				}
			}

			//get rid of the super close together verts    TODO is this correct/safe for hashing?
			List<Vector2> uniqueVerts = new List<Vector2>();
			foreach (var v in profileVerts)
			{
				if (!uniqueVerts.Any(existing => Vector3.Distance(existing, v) < comparisonTolerance))
				{
					uniqueVerts.Add(v);
				}
			}

			//sort them so identical profiles are always in the same order
			List<Vector2> sortedVerts = uniqueVerts
		  .OrderBy(v => Mathf.Round(v.x / comparisonTolerance))
		  .ThenBy(v => Mathf.Round(v.y / comparisonTolerance))
		  .ToList();

			Profile face = new Profile(sortedVerts);
			string faceKey = face.GetKey();

			if (!profileToEdgeName.ContainsKey(faceKey))
			{
				profileToEdgeName.Add(faceKey, nextEdgeName);
				SaveRotations(face, (profileAxis == 1));
				nextEdgeName++;
			}
			return profileToEdgeName[faceKey];
		}
		private void SaveRotations(Profile face, bool isVert)
		{
			//generate the rotated versions and save them to profileToEdgeName
			if (isVert)
			{
				//if not symmetrical, generate rotational indexes
			}
			else
			{
				//if not symmetrical, generate flipped version
			}
		}
	}

	[CustomEditor(typeof(EdgeData))]
	public class EdgeDataUtil : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EdgeData myScript = (EdgeData)target;
			if (GUILayout.Button("Generate Data"))
			{
				myScript.GenerateData();
			}

			if (myScript.profileToEdgeName != null)
			{
				GUILayout.Label("EDGES");
				GUILayout.Label("________________________________________________________________________");
				foreach (var (key, value) in myScript.profileToEdgeName)
				{
					GUILayout.Label(value.ToString());
					GUILayout.TextArea(key);
				}
			}
		}
	}
#endif
}