using UnityEngine;

public class ArbitraryCableDrawer : MonoBehaviour
{
	public Transform Attach1;

	public Vector2 Attach1Offset;

	public Transform Attach2;

	public Vector2 Attach2Offset;

	private Mesh m_mesh;

	private Vector3[] m_vertices;

	private MeshFilter m_stringFilter;

	public void Initialize(Transform t1, Transform t2)
	{
		Attach1 = t1;
		Attach2 = t2;
		m_mesh = new Mesh();
		m_vertices = new Vector3[20];
		m_mesh.vertices = m_vertices;
		int[] array = new int[54];
		Vector2[] uv = new Vector2[20];
		int num = 0;
		for (int i = 0; i < 9; i++)
		{
			array[i * 6] = num;
			array[i * 6 + 1] = num + 2;
			array[i * 6 + 2] = num + 1;
			array[i * 6 + 3] = num + 2;
			array[i * 6 + 4] = num + 3;
			array[i * 6 + 5] = num + 1;
			num += 2;
		}
		m_mesh.triangles = array;
		m_mesh.uv = uv;
		GameObject gameObject = new GameObject("cableguy");
		m_stringFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = BraveResources.Load("Global VFX/WhiteMaterial", ".mat") as Material;
		meshRenderer.material.SetColor("_OverrideColor", Color.black);
		m_stringFilter.mesh = m_mesh;
	}

	private void LateUpdate()
	{
		if ((bool)Attach1 && (bool)Attach2)
		{
			Vector3 vector = Attach1.position.XY().ToVector3ZisY(-3f) + Attach1Offset.ToVector3ZisY();
			Vector3 vector2 = Attach2.position.XY().ToVector3ZisY(-3f) + Attach2Offset.ToVector3ZisY();
			BuildMeshAlongCurve(vector, vector, vector2 + new Vector3(0f, -2f, -2f), vector2);
			m_mesh.vertices = m_vertices;
			m_mesh.RecalculateBounds();
			m_mesh.RecalculateNormals();
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_stringFilter)
		{
			Object.Destroy(m_stringFilter.gameObject);
		}
	}

	private void BuildMeshAlongCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float meshWidth = 1f / 32f)
	{
		Vector3[] vertices = m_vertices;
		Vector2? vector = null;
		for (int i = 0; i < 10; i++)
		{
			Vector2 vector2 = BraveMathCollege.CalculateBezierPoint((float)i / 9f, p0, p1, p2, p3);
			Vector2? vector3 = ((i != 9) ? new Vector2?(BraveMathCollege.CalculateBezierPoint((float)i / 9f, p0, p1, p2, p3)) : null);
			Vector2 zero = Vector2.zero;
			if (vector.HasValue)
			{
				zero += (Quaternion.Euler(0f, 0f, 90f) * (vector2 - vector.Value)).XY().normalized;
			}
			if (vector3.HasValue)
			{
				zero += (Quaternion.Euler(0f, 0f, 90f) * (vector3.Value - vector2)).XY().normalized;
			}
			zero = zero.normalized;
			vertices[i * 2] = (vector2 + zero * meshWidth).ToVector3ZisY();
			vertices[i * 2 + 1] = (vector2 + -zero * meshWidth).ToVector3ZisY();
			vector = vector2;
		}
	}
}
