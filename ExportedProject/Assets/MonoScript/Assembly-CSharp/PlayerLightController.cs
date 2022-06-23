using System.Collections.Generic;
using UnityEngine;

public class PlayerLightController : MonoBehaviour
{
	public int resolution = 1000;

	public float maxDistance = 10f;

	public float distortionMax = 0.5f;

	public Material shadowMaterial;

	private MeshFilter mf;

	private MeshRenderer mr;

	private Mesh m;

	private List<Vector3> vertices;

	private List<int> triangles;

	private List<Vector2> uvs;

	private Vector3[] directionCache;

	private int layerMask = -1025;

	private void Start()
	{
		mf = GetComponent<MeshFilter>();
		if (mf == null)
		{
			mf = base.gameObject.AddComponent<MeshFilter>();
		}
		mr = GetComponent<MeshRenderer>();
		if (mr == null)
		{
			mr = base.gameObject.AddComponent<MeshRenderer>();
		}
		vertices = new List<Vector3>();
		triangles = new List<int>();
		uvs = new List<Vector2>();
		directionCache = new Vector3[resolution];
		CacheDirections();
		UpdateVertices(true);
		m = new Mesh();
		m.vertices = vertices.ToArray();
		m.triangles = triangles.ToArray();
		m.uv = uvs.ToArray();
		m.RecalculateBounds();
		m.RecalculateNormals();
		mf.sharedMesh = m;
		mr.material = shadowMaterial;
	}

	private void CacheDirections()
	{
		for (int i = 0; i < resolution; i++)
		{
			float z = (float)i * (360f / (float)resolution);
			Vector3 vector = Quaternion.Euler(0f, 0f, z) * Vector3.up;
			directionCache[i] = vector.normalized;
		}
	}

	private void UpdateVertices(bool generateTrisAndUVs)
	{
		vertices.Clear();
		if (generateTrisAndUVs)
		{
			triangles.Clear();
			uvs.Clear();
		}
		for (int i = 0; i < resolution; i++)
		{
			Ray ray = new Ray(base.transform.position, directionCache[i]);
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 point;
			Vector3 point2;
			float num;
			if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
			{
				point = hitInfo.point;
				point2 = ray.GetPoint(maxDistance + 1f);
				num = Mathf.Max(hitInfo.distance / maxDistance, 0.5f);
				num = Mathf.Clamp01(1f - num);
			}
			else
			{
				point = ray.GetPoint(maxDistance);
				point2 = ray.GetPoint(maxDistance + 1f);
				num = 0f;
			}
			vertices.Add(base.transform.InverseTransformPoint(point) + directionCache[i] * (distortionMax * num));
			vertices.Add(base.transform.InverseTransformPoint(point2));
			if (generateTrisAndUVs)
			{
				if (i > 1)
				{
					triangles.Add(i * 2 - 1);
					triangles.Add(i * 2 - 2);
					triangles.Add(i * 2);
					triangles.Add(i * 2);
					triangles.Add(i * 2 + 1);
					triangles.Add(i * 2 - 1);
				}
				uvs.Add(Vector2.zero);
				uvs.Add(Vector2.zero);
			}
		}
		if (generateTrisAndUVs)
		{
			triangles.Add(vertices.Count - 1);
			triangles.Add(vertices.Count - 2);
			triangles.Add(0);
			triangles.Add(0);
			triangles.Add(1);
			triangles.Add(vertices.Count - 1);
		}
	}

	private void LateUpdate()
	{
		UpdateVertices(false);
		m.vertices = vertices.ToArray();
	}
}
