using UnityEngine;

public class BalloonAttachmentDoer : MonoBehaviour
{
	public GameActor AttachTarget;

	private Vector2 m_currentOffset;

	private Mesh m_mesh;

	private Vector3[] m_vertices;

	private tk2dSprite m_sprite;

	private MeshFilter m_stringFilter;

	public void Initialize(GameActor target)
	{
		AttachTarget = target;
		m_currentOffset = new Vector2(1f, 2f);
		m_sprite = GetComponent<tk2dSprite>();
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
		GameObject gameObject = new GameObject("balloon string");
		m_stringFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = BraveResources.Load("Global VFX/WhiteMaterial", ".mat") as Material;
		m_stringFilter.mesh = m_mesh;
		base.transform.position = AttachTarget.transform.position + m_currentOffset.ToVector3ZisY(-3f);
	}

	private void LateUpdate()
	{
		if (AttachTarget != null)
		{
			if (AttachTarget is AIActor && (!AttachTarget || AttachTarget.healthHaver.IsDead))
			{
				Object.Destroy(base.gameObject);
				return;
			}
			m_currentOffset = new Vector2(Mathf.Lerp(0.5f, 1f, Mathf.PingPong(Time.realtimeSinceStartup, 3f) / 3f), Mathf.Lerp(1.33f, 2f, Mathf.PingPong(Time.realtimeSinceStartup / 1.75f, 3f) / 3f));
			Vector3 vector = AttachTarget.CenterPosition;
			if (AttachTarget is PlayerController)
			{
				PlayerHandController primaryHand = (AttachTarget as PlayerController).primaryHand;
				if (primaryHand.renderer.enabled)
				{
					vector = primaryHand.sprite.WorldCenter;
				}
			}
			Vector3 vector2 = AttachTarget.transform.position + m_currentOffset.ToVector3ZisY(-3f);
			float num = Vector3.Distance(base.transform.position, vector2);
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector2, BraveMathCollege.UnboundedLerp(1f, 10f, num / 3f) * BraveTime.DeltaTime);
			BuildMeshAlongCurve(vector, vector, m_sprite.WorldCenter + new Vector2(0f, -2f), m_sprite.WorldCenter);
			m_mesh.vertices = m_vertices;
			m_mesh.RecalculateBounds();
			m_mesh.RecalculateNormals();
		}
		if (!AttachTarget || AttachTarget.healthHaver.IsDead)
		{
			Object.Destroy(base.gameObject);
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
