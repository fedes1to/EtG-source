using Dungeonator;
using UnityEngine;

public class ScarfAttachmentDoer : MonoBehaviour
{
	public float StartWidth = 0.0625f;

	public float EndWidth = 0.125f;

	public float AnimationSpeed = 1f;

	public float ScarfLength = 1.5f;

	public float AngleLerpSpeed = 15f;

	public float ForwardZOffset;

	public float BackwardZOffset = -0.2f;

	public Vector2 MinOffset;

	public Vector2 MaxOffset;

	public float CatchUpScale = 2f;

	public GameActor AttachTarget;

	public Material ScarfMaterial;

	private float m_additionalOffsetTime;

	private Vector2 m_currentOffset;

	private Mesh m_mesh;

	private Vector3[] m_vertices;

	private MeshFilter m_stringFilter;

	private MeshRenderer m_mr;

	private float m_lastVelAngle;

	private bool m_isLerpingBack;

	private float m_targetLength;

	private float m_currentLength = 0.05f;

	public float SinSpeed = 1f;

	public float AmplitudeMod = 0.25f;

	public float WavelengthMod = 1f;

	public void Initialize(GameActor target)
	{
		m_targetLength = ScarfLength;
		AttachTarget = target;
		m_currentOffset = new Vector2(1f, 2f);
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
		m_mr = gameObject.AddComponent<MeshRenderer>();
		m_mr.material = ScarfMaterial;
		Object.DontDestroyOnLoad(base.gameObject);
		Object.DontDestroyOnLoad(gameObject);
		m_stringFilter.mesh = m_mesh;
		base.transform.position = AttachTarget.transform.position + m_currentOffset.ToVector3ZisY(-3f);
	}

	private void LateUpdate()
	{
		if (GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating || !(AttachTarget != null))
		{
			return;
		}
		if (AttachTarget is AIActor && (!AttachTarget || AttachTarget.healthHaver.IsDead))
		{
			Object.Destroy(base.gameObject);
			return;
		}
		m_targetLength = ScarfLength;
		bool flag = false;
		if (AttachTarget is PlayerController)
		{
			PlayerController playerController = AttachTarget as PlayerController;
			m_mr.enabled = playerController.IsVisible && playerController.sprite.renderer.enabled;
			m_mr.gameObject.layer = playerController.gameObject.layer;
			if (playerController.FacingDirection <= 155f && playerController.FacingDirection >= 25f)
			{
				flag = true;
			}
			if (playerController.IsFalling)
			{
				m_targetLength = 0.05f;
			}
		}
		m_currentLength = Mathf.MoveTowards(m_currentLength, m_targetLength, BraveTime.DeltaTime * 2.5f);
		if (m_currentLength < 0.1f)
		{
			m_mr.enabled = false;
		}
		Vector2 lastCommandedDirection = (AttachTarget as PlayerController).LastCommandedDirection;
		if (lastCommandedDirection.magnitude < 0.125f)
		{
			m_isLerpingBack = true;
		}
		else
		{
			m_isLerpingBack = false;
		}
		float lastVelAngle = m_lastVelAngle;
		if (m_isLerpingBack)
		{
			float num = Mathf.DeltaAngle(m_lastVelAngle, -45f);
			float num2 = Mathf.DeltaAngle(m_lastVelAngle, 135f);
			float num3 = ((num > num2) ? 180 : 0);
			lastVelAngle = num3;
		}
		else
		{
			lastVelAngle = BraveMathCollege.Atan2Degrees(lastCommandedDirection);
		}
		m_lastVelAngle = Mathf.LerpAngle(m_lastVelAngle, lastVelAngle, BraveTime.DeltaTime * AngleLerpSpeed * Mathf.Lerp(1f, 2f, Mathf.DeltaAngle(m_lastVelAngle, lastVelAngle) / 180f));
		float num4 = m_currentLength * Mathf.Lerp(2f, 1f, Vector2.Distance(base.transform.position.XY(), AttachTarget.sprite.WorldCenter) / 3f);
		m_currentOffset = (Quaternion.Euler(0f, 0f, m_lastVelAngle) * Vector2.left * num4).XY();
		Vector2 vector = Vector2.Lerp(MinOffset, MaxOffset, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.realtimeSinceStartup * AnimationSpeed, 3f) / 3f));
		m_currentOffset += vector;
		Vector3 vector2 = AttachTarget.sprite.WorldCenter + new Vector2(0f, -0.3125f);
		Vector3 vector3 = vector2 + m_currentOffset.ToVector3ZisY(-3f);
		float num5 = Vector3.Distance(base.transform.position, vector3);
		if (num5 > 10f)
		{
			base.transform.position = vector3;
		}
		else
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, vector3, BraveMathCollege.UnboundedLerp(1f, 10f, num5 / CatchUpScale) * BraveTime.DeltaTime);
		}
		Vector2 vector4 = vector3.XY() - base.transform.position.XY();
		m_additionalOffsetTime += Random.Range(0f, BraveTime.DeltaTime);
		BuildMeshAlongCurve(vector2, vector2.XY() + new Vector2(0f, 0.1f), base.transform.position.XY() + vector4, base.transform.position.XY(), (!flag) ? ForwardZOffset : BackwardZOffset);
		m_mesh.vertices = m_vertices;
		m_mesh.RecalculateBounds();
		m_mesh.RecalculateNormals();
	}

	private void OnDestroy()
	{
		if ((bool)m_stringFilter)
		{
			Object.Destroy(m_stringFilter.gameObject);
		}
	}

	private void BuildMeshAlongCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float zOffset)
	{
		Vector3[] vertices = m_vertices;
		Vector2? vector = null;
		Vector2 vector2 = p3 - p0;
		Vector2 vector3 = (Quaternion.Euler(0f, 0f, 90f) * vector2).XY();
		for (int i = 0; i < 10; i++)
		{
			Vector2 vector4 = BraveMathCollege.CalculateBezierPoint((float)i / 9f, p0, p1, p2, p3);
			Vector2? vector5 = ((i != 9) ? new Vector2?(BraveMathCollege.CalculateBezierPoint((float)i / 9f, p0, p1, p2, p3)) : null);
			Vector2 zero = Vector2.zero;
			if (vector.HasValue)
			{
				zero += (Quaternion.Euler(0f, 0f, 90f) * (vector4 - vector.Value)).XY().normalized;
			}
			if (vector5.HasValue)
			{
				zero += (Quaternion.Euler(0f, 0f, 90f) * (vector5.Value - vector4)).XY().normalized;
			}
			zero = zero.normalized;
			float num = Mathf.Lerp(StartWidth, EndWidth, (float)i / 9f);
			vector4 += vector3.normalized * Mathf.Sin(Time.realtimeSinceStartup * SinSpeed + (float)i * WavelengthMod) * AmplitudeMod * ((float)i / 9f);
			vertices[i * 2] = (vector4 + zero * num).ToVector3ZisY(zOffset);
			vertices[i * 2 + 1] = (vector4 + -zero * num).ToVector3ZisY(zOffset);
			vector = vector4;
		}
	}
}
