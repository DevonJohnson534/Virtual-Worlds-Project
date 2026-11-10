using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BuildingGen {
#if UNITY_EDITOR
	[CreateAssetMenu(fileName = "EdgeData", menuName = "Scriptable Objects/EdgeData")]
	public class EdgeData : ScriptableObject
	{
		private const string TilePrototypesDirectory = "Assets/Building Gen/Prefabs/Tile prototypes";
		private const int edgeDist = 1; //this is the distance from the center where the edges are. aka half of tile size
		private const float comparisonTolerance = 0.0005f; //used when comparing floats

		public Dictionary<string, EdgeInfo> profileToEdgeName { get; private set; }
		private int nextEdgeName;
		public class Profile
		{
			//a "Profile" is all the verticies on an "Edge" which is one of a mesh's 6 boundary box faces
			public List<Vector2> verts;
			public bool isHorizontal;
			public Profile(List<Vector2> _verts, bool _isHorizontal)
			{
				verts = _verts;
				isHorizontal = _isHorizontal;
			}
			public string GetKey() //this is for the profileToEdgeName dictionary keys
			{
				if (verts == null || verts.Count == 0) return "EMPTY";

				string dir_str = isHorizontal ? "h\n" : "v\n";

				//round to tolerance and cut it to 4 decimal places. put in readable format
				string verts_str = string.Join("\n", verts.Select(v =>
					 $"({Mathf.Round(v.x / comparisonTolerance) * comparisonTolerance:F4}," +
					 $"{Mathf.Round(v.y / comparisonTolerance) * comparisonTolerance:F4})"));

				return dir_str + verts_str;
			}
		}
		public void GenerateData()
		{
			//labels all the tile prototype prefabs with their correct edge data

			Debug.Log("Starting data generation");
			profileToEdgeName = new();
			//add the empty profile as name 0. vertical or horizontal doesn't matter here
			profileToEdgeName.Add(new Profile(new(), false).GetKey(), new EdgeInfo(0,-1));
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
					for (int i = 0; i < 6; i++)
					{
						tileProto.edges[i] = new EdgeInfo(0, -1);
					}
				}

				EditorUtility.SetDirty(tileProto); //this means that changes made to the prefab's TilePrototype can be saved
			}

			//AssetDatabase.SaveAssets(); //saves the edited prefabs automatically
			Debug.Log("Data generation complete!");
		}

		private void SetEdgeInfo(string assetName, TilePrototype tileProto, Mesh mesh)
		{
			tileProto.edges[Directions.X_POS] = FindEdgeName(assetName, mesh, 0, 1);
			tileProto.edges[Directions.X_NEG] = FindEdgeName(assetName, mesh, 0, -1);

			tileProto.edges[Directions.Y_POS] = FindEdgeName(assetName, mesh, 1, 1);
			tileProto.edges[Directions.Y_NEG] = FindEdgeName(assetName, mesh, 1, -1);

			tileProto.edges[Directions.Z_POS] = FindEdgeName(assetName, mesh, 2, 1);
			tileProto.edges[Directions.Z_NEG] = FindEdgeName(assetName, mesh, 2, -1);
		}
		private Dictionary<int, (int,int,int)> uvFromProfileAxis = new(){ {0, (2,1,1)}, {1, (2,0,-1)}, { 2, (0,1,-1) } }; //axis => u, v, udir
		//u is left/right and v is up/down

		private EdgeInfo FindEdgeName(string name, Mesh mesh, int profileAxis, int profileDir)
		{
			//profileAxis is 0,1,2 meaning x,y,z    profileDir is -1 or 1 meaning if it is the positive or negative side
			List<Vector3> allVerts = new();
			mesh.GetVertices(allVerts);

			int uAxis = uvFromProfileAxis[profileAxis].Item1;
			int vAxis = uvFromProfileAxis[profileAxis].Item2;

			int uDir = uvFromProfileAxis[profileAxis].Item3;
			if (profileDir == -1 && profileAxis != 1 ) { uDir = -uDir; } //only flip u for the x and z axis

			//get the profile and store it as 2D
			List<Vector2> profileVerts = new();
			for (int i = 0; i < allVerts.Count; i++)
			{
				Vector3 vert = allVerts[i];
				if (math.abs(vert[profileAxis] - profileDir * edgeDist) < comparisonTolerance)
				{
					float u = vert[uAxis] * uDir;
					float v = vert[vAxis];

					profileVerts.Add(new Vector2(u, v));
				}
			}

			//get rid of the super close together verts    TODO is this correct/safe for hashing? I think so? actually maybe not
			List<Vector2> uniqueVerts = new List<Vector2>();
			foreach (var v in profileVerts)
			{
				if (!uniqueVerts.Any(existing => Vector3.Distance(existing, v) < comparisonTolerance))
				{
					uniqueVerts.Add(v);
				}
			}

			//sort them so identical profiles are always in the same order
			List<Vector2> sortedVerts = sortVerts(uniqueVerts);

			Profile face = new Profile(sortedVerts, profileAxis!=1);
			string faceKey = face.GetKey();

			if (!profileToEdgeName.ContainsKey(faceKey))
			{
				int edgeName = nextEdgeName;
				nextEdgeName++;

				bool hasRotations = SaveRotations(face, edgeName);
				int ogRot = hasRotations ? 0 : -1;
				profileToEdgeName.Add(faceKey, new EdgeInfo(edgeName, ogRot));
			}
			return profileToEdgeName[faceKey];
		}
		private List<Vector2> sortVerts(List<Vector2> input)
		{
			//doesn't edit the origional
			List<Vector2> sortedVerts = input
			.OrderBy(v => Mathf.Round(v.x / comparisonTolerance))
			.ThenBy(v => Mathf.Round(v.y / comparisonTolerance))
			.ToList();
			return sortedVerts;
		}
		private bool SaveRotations(Profile face, int edgeName)
		{
			//generate any rotated versions and save them to profileToEdgeName.
			//return False if nothing was generated, meaning that it's symmetrical
			if (face.isHorizontal)
			{
				//if not symmetrical, generate flipped version
				return RotateHorizontal(face, edgeName);
			}
			else
			{
				//if not symmetrical, generate all rotated versions
				return RotateVertical(face, edgeName);
			}
		}
		private bool RotateVertical(Profile face, int edgeName) //TODO handle the situtations where it is symmetrical at 180 degrees but not 90
		{
			bool symmetrical = true;
			List<Vector2> previousVerts = face.verts;
			for (int i = 1; i < 4; i++) //i = 1,2,3
			{
				List<Vector2> rotatedVerts = new List<Vector2>();
				foreach (Vector2 vert in previousVerts)
				{
					rotatedVerts.Add(new Vector2(vert.y, -vert.x)); //rotate 90 degrees clockwise
				}
				List<Vector2> sortedRotatedVerts = sortVerts(rotatedVerts);

				Profile rotFace = new Profile(sortedRotatedVerts, false);
				string rotFaceKey = rotFace.GetKey();
				if (rotFaceKey != face.GetKey())
				{
					symmetrical = false;
					profileToEdgeName.TryAdd(rotFaceKey, new EdgeInfo(edgeName, i));
				}
				previousVerts = rotatedVerts; //save the previous rotation in order to keep rotating it
			}
			return !symmetrical;
		}
		private bool RotateHorizontal(Profile face, int edgeName)
		{
			List<Vector2> rotatedVerts = new List<Vector2>();
			foreach (Vector2 vert in face.verts)
			{
				rotatedVerts.Add(new Vector2(vert.x * -1, vert.y));
			}
			List<Vector2> sortedRotatedVerts = sortVerts(rotatedVerts);

			Profile rotFace = new Profile(sortedRotatedVerts, true);

			string rotFaceKey = rotFace.GetKey();
			if (rotFaceKey == face.GetKey()) return false; //symmetrical. don't add anything and return false

			profileToEdgeName.Add(rotFaceKey, new EdgeInfo(edgeName, 1)); //not symmetrical. add the rotation and return true
			return true;
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