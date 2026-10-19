using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR //TODO is this correct usage?
using UnityEditor;
#endif

namespace BuildingGen {
	[CreateAssetMenu(fileName = "EdgeData", menuName = "Scriptable Objects/EdgeData")]
	public class EdgeData : ScriptableObject
	{
		private const string TilePrototypesDirectory = "Assets/Building Gen/Prefabs/Tile prototypes";
		private const int edgeDist = 1; //this is the distance from the center where the edges are. aka half of tile size
		private const float comparisonTolerance = 0.0005f; //used when comparing floats

		public Dictionary<Profile, int> profileToEdgeName { get; private set; }
		private int nextEdgeName;
		public class Profile
		{
			public List<Vector3> verts;
			public Profile(List<Vector3> _verts)
			{
				verts = _verts;
			}
		}
		public class FaceEqualityComparer : IEqualityComparer<Profile>
		{
			public bool Equals(Profile f1, Profile f2)
			{
				if (ReferenceEquals(f1, f2))
					return true;

				if (f2 is null || f1 is null)
					return false;

				if (f2.verts.Count != f1.verts.Count)
					return false;

				for (int i = 0; i < f1.verts.Count; i++)
				{
					if (Vector3.Distance(f1.verts[i], f2.verts[i]) > comparisonTolerance) return false;
				}
				return true;
			}
			public int GetHashCode(Profile profile)
			{
				Hash128 code = new();
				for (int i = 0;i < profile.verts.Count; i++)
				{
					Vector3 v = profile.verts[i];
					HashUtilities.QuantisedVectorHash(ref v, ref code); //uses a step of 0.0005
				}
				return code.GetHashCode();
			}
		}
		public class VertEqualityComparer : IEqualityComparer<Vector3>
		{
			public bool Equals(Vector3 v1, Vector3 v2)
			{
				if (ReferenceEquals(v1, v2))
					return true;

				return (Vector3.Distance(v1, v2) < comparisonTolerance);
			}
			public int GetHashCode(Vector3 v)
			{
				Hash128 code = new();
				HashUtilities.QuantisedVectorHash(ref v, ref code); //uses a step of 0.0005
				return code.GetHashCode();
			}
		}

		public void GenerateData()
		{
			profileToEdgeName = new Dictionary<Profile, int>(new FaceEqualityComparer());
			profileToEdgeName.Add(new Profile(new()), 0); //add the empty profile as edge name 0
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
					Debug.Log(asset.name + " has no mesh filter. adding empty edges.");
					tileProto.xMinusEdge.edgeName = 0;
					tileProto.xPlusEdge.edgeName = 0;
					tileProto.yMinusEdge.edgeName = 0;
					tileProto.yPlusEdge.edgeName = 0;
					tileProto.zMinusEdge.edgeName = 0;
					tileProto.zPlusEdge.edgeName = 0;
				}
			}
			Debug.Log("Data generation is complete!");
		}

		private void SetEdgeInfo(string assetName, TilePrototype tileProto, Mesh mesh)
		{
			tileProto.xPlusEdge.edgeName = profileToEdgeName[GetMeshProfile(assetName, mesh, 0, 1)];
			tileProto.xMinusEdge.edgeName = profileToEdgeName[GetMeshProfile(assetName, mesh, 0, -1)];

			tileProto.yPlusEdge.edgeName = profileToEdgeName[GetMeshProfile(assetName, mesh, 1, 1)];
			tileProto.yMinusEdge.edgeName = profileToEdgeName[GetMeshProfile(assetName, mesh, 1, -1)];

			tileProto.zPlusEdge.edgeName = profileToEdgeName[GetMeshProfile(assetName, mesh, 2, 1)];
			tileProto.zMinusEdge.edgeName = profileToEdgeName[GetMeshProfile(assetName, mesh, 2, -1)];
		}

		private Profile GetMeshProfile(string name, Mesh mesh, int profileAxis, int profileDir)
		{
			//faceAxis is 0,1,2 meaning x,y,z    faceDir is -1 or 1 meaning the way the face is facing
			List<Vector3> allVerts = new();
			mesh.GetVertices(allVerts);

			List<Vector3> profileVerts = new();

			for (int i = 0; i < allVerts.Count; i++)
			{
				Vector3 vert = allVerts[i];
				if (math.abs(vert[profileAxis] - profileDir* edgeDist) < comparisonTolerance)
				{
					//store them all facing the same way, which is faceaxis=0 and facedir=1 aka posX direction
					profileVerts.Add(RotateVert(vert, profileAxis, profileDir));
				}
			}
			Profile face = new Profile(profileVerts
											 .Distinct(new VertEqualityComparer())
											 .OrderBy(v => v.x)
											 .ThenBy(v => v.y)
											 .ThenBy(v => v.z)
											 .ToList());
			Debug.Log("axis: "+ profileAxis + " dir: "+ profileDir);
			foreach (Vector3 vert in face.verts)
			{
				Debug.Log("     " + vert.x + "   " + vert.y + "   " + vert.z);
			}

			if (!profileToEdgeName.ContainsKey(face))
			{
				profileToEdgeName.Add(face, nextEdgeName);
				nextEdgeName++;
			}
			return face;
		}

		private Vector3 RotateVert(Vector3 vert, int profileAxis, int profileDir)
		{
			float3x3 mat = float3x3.identity;
			switch ((profileAxis+1) * profileDir)
			{
				case 1: //+X
					mat = float3x3.identity;
					break;
				case -1: //-X
					mat = new float3x3(-1, 0, 0,
											 0, 1, 0,
											 0, 0, 1);
					break;
				case 2:  //+Y
					mat = new float3x3(0, 1, 0,
											 -1, 0, 0,
											 0, 0, 1);
					break;
				case -2: //-Y
					mat = new float3x3(0, -1, 0,
											 1, 0, 0,
											 0, 0, 1);
					break;
				case 3:  //+Z
					mat = new float3x3(0, 0, 1,
											 0, 1, 0,
											 -1, 0, 0);
					break;
				case -3: //-Z
					mat = new float3x3(0, 0, -1,
											 0, 1, 0,
											 1, 0, 0);
					break;
				default:
					Debug.Log("ERROR ROTATING axis:"+profileAxis+" dir:"+profileDir);
					break;
			}
			return math.mul(mat, vert);
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
					string text = "";
					string warnings = "";
					foreach (Vector3 vert in key.verts)
					{
						if (vert.x != 1) warnings += ("WARNING X!=1  instead x="+vert.x+"\n");
						text += ("("+vert.y + "," + vert.z+ ")\n");
					}
					GUILayout.TextArea(warnings+"\n"+ text);
				}
			}
		}
	}
}