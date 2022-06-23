using AK.Wwise;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Wwise/AkSurfaceReflector")]
public class AkSurfaceReflector : MonoBehaviour
{
	public AcousticTexture AcousticTexture;

	private MeshFilter MeshFilter;

	public static void AddGeometrySet(AcousticTexture acousticTexture, MeshFilter meshFilter)
	{
		if (meshFilter == null)
		{
			Debug.Log(meshFilter.name + ": No mesh found!");
			return;
		}
		Mesh sharedMesh = meshFilter.sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		int[] triangles = sharedMesh.triangles;
		int num = sharedMesh.triangles.Length / 3;
		using (AkTriangleArray akTriangleArray = new AkTriangleArray(num))
		{
			for (int i = 0; i < num; i++)
			{
				using (AkTriangle akTriangle = akTriangleArray.GetTriangle(i))
				{
					Vector3 vector = meshFilter.transform.TransformPoint(vertices[triangles[3 * i]]);
					Vector3 vector2 = meshFilter.transform.TransformPoint(vertices[triangles[3 * i + 1]]);
					Vector3 vector3 = meshFilter.transform.TransformPoint(vertices[triangles[3 * i + 2]]);
					akTriangle.point0.X = vector.x;
					akTriangle.point0.Y = vector.y;
					akTriangle.point0.Z = vector.z;
					akTriangle.point1.X = vector2.x;
					akTriangle.point1.Y = vector2.y;
					akTriangle.point1.Z = vector2.z;
					akTriangle.point2.X = vector3.x;
					akTriangle.point2.Y = vector3.y;
					akTriangle.point2.Z = vector3.z;
					akTriangle.textureID = (uint)acousticTexture.ID;
					akTriangle.reflectorChannelMask = uint.MaxValue;
					akTriangle.strName = meshFilter.gameObject.name + "_" + i;
				}
			}
			AkSoundEngine.SetGeometry((ulong)meshFilter.GetInstanceID(), akTriangleArray, (uint)num);
		}
	}

	public static void RemoveGeometrySet(MeshFilter meshFilter)
	{
		if (meshFilter != null)
		{
			AkSoundEngine.RemoveGeometry((ulong)meshFilter.GetInstanceID());
		}
	}

	private void Awake()
	{
		MeshFilter = GetComponent<MeshFilter>();
	}

	private void OnEnable()
	{
		AddGeometrySet(AcousticTexture, MeshFilter);
	}

	private void OnDisable()
	{
		RemoveGeometrySet(MeshFilter);
	}
}
