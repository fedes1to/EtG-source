using UnityEngine;

public class BulletArcLightningController : MonoBehaviour
{
	private Vector2 m_center;

	private float m_velocity;

	private float m_startAngle;

	private float m_endAngle;

	private float m_currentRadius;

	private string m_ownerName;

	private LineRenderer m_line;

	private Vector3[] m_linePoints;

	public void Initialize(Vector2 centerPoint, float velocity, string OwnerName, float startAngle = 0f, float endAngle = 360f, float startRadius = 0f)
	{
		m_ownerName = OwnerName;
		m_center = centerPoint;
		m_velocity = velocity;
		m_startAngle = startAngle;
		m_endAngle = endAngle;
		m_currentRadius = startRadius;
		m_line = base.gameObject.GetOrAddComponent<LineRenderer>();
		m_linePoints = new Vector3[16];
		m_line.SetVertexCount(m_linePoints.Length);
		m_line.SetPositions(m_linePoints);
		m_line.SetWidth(1f, 1f);
		m_line.material = BraveResources.Load("Global VFX/ArcLightningMaterial", ".mat") as Material;
		while (m_startAngle > m_endAngle)
		{
			m_endAngle += 360f;
		}
	}

	public void UpdateCenter(Vector2 newCenter)
	{
		m_center = newCenter;
	}

	public void Update()
	{
		float num = m_velocity * BraveTime.DeltaTime;
		m_currentRadius += num;
		UpdateRendering();
		UpdateCollision();
	}

	public void OnDespawned()
	{
		if ((bool)m_line)
		{
			for (int i = 0; i < m_linePoints.Length; i++)
			{
				m_linePoints[i] = Vector3.zero;
			}
			m_line.SetPositions(m_linePoints);
			Object.Destroy(m_line);
		}
		m_line = null;
		m_linePoints = null;
		Object.Destroy(this);
	}

	private void UpdateRendering()
	{
		float num = m_endAngle - m_startAngle;
		float num2 = num / (float)m_linePoints.Length;
		for (int i = 0; i < m_linePoints.Length; i++)
		{
			m_linePoints[i] = (m_center + BraveMathCollege.DegreesToVector(m_startAngle + (float)i * num2, m_currentRadius)).ToVector3ZisY();
		}
		m_line.SetPositions(m_linePoints);
	}

	private bool IsBetweenAngles(Vector2 circleCenter, Vector2 point, float startAngle, float endAngle)
	{
		float num = ((point - circleCenter).ToAngle() + 360f) % 360f;
		return endAngle >= num && startAngle <= num;
	}

	public bool ArcIntersectsLine(Vector2 circleCenter, float radius, float startAngle, float endAngle, Vector2 point1, Vector2 point2)
	{
		Vector2 vector = point1 - circleCenter;
		Vector2 vector2 = point2 - circleCenter;
		Vector2 vector3 = vector2 - vector;
		float num = Vector2.Dot(vector3, vector3);
		float num2 = 2f * Vector2.Dot(vector, vector3);
		float num3 = Vector2.Dot(vector, vector) - radius * radius;
		float num4 = Mathf.Pow(num2, 2f) - 4f * num * num3;
		if (num4 > 0f)
		{
			float num5 = ((!(num2 >= 0f)) ? ((0f - num2 + Mathf.Sqrt(num4)) / 2f) : ((0f - num2 - Mathf.Sqrt(num4)) / 2f));
			float num6 = num5 / num;
			float num7 = num3 / num5;
			if (num7 < num6)
			{
				float num8 = num7;
				num7 = num6;
				num6 = num8;
			}
			if (0.0 <= (double)num6 && (double)num6 <= 1.0)
			{
				Vector2 point3 = circleCenter + Vector2.Lerp(vector, vector2, num6);
				if (IsBetweenAngles(circleCenter, point3, startAngle, endAngle))
				{
					return true;
				}
			}
			if (0.0 <= (double)num7 && (double)num7 <= 1.0)
			{
				Vector2 point4 = circleCenter + Vector2.Lerp(vector, vector2, num7);
				if (IsBetweenAngles(circleCenter, point4, startAngle, endAngle))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private bool ArcSliceIntersectsAABB(Vector2 centerPoint, float startAngle, float endAngle, float startRadius, float endRadius, Vector2 aabbBottomLeft, Vector2 aabbTopRight)
	{
		Vector2 point = aabbBottomLeft;
		Vector2 point2 = aabbTopRight;
		Vector2 vector = new Vector2(point2.x, point.y);
		Vector2 vector2 = new Vector2(point.x, point2.y);
		bool flag = ArcIntersectsLine(centerPoint, endRadius, startAngle, endAngle, point, vector);
		if (!flag)
		{
			flag = ArcIntersectsLine(centerPoint, endRadius, startAngle, endAngle, vector, point2);
		}
		if (!flag)
		{
			flag = ArcIntersectsLine(centerPoint, endRadius, startAngle, endAngle, vector2, point2);
		}
		if (!flag)
		{
			flag = ArcIntersectsLine(centerPoint, endRadius, startAngle, endAngle, point, vector2);
		}
		return flag;
	}

	private void UpdateCollision()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && !playerController.IsGhost && (bool)playerController.healthHaver && playerController.healthHaver.IsAlive && playerController.healthHaver.IsVulnerable)
			{
				Vector2 zero = Vector2.zero;
				if (ArcSliceIntersectsAABB(m_center, m_startAngle, m_endAngle, m_currentRadius, m_currentRadius, playerController.specRigidbody.HitboxPixelCollider.UnitBottomLeft, playerController.specRigidbody.HitboxPixelCollider.UnitTopRight))
				{
					playerController.healthHaver.ApplyDamage(0.5f, Vector2.zero, m_ownerName);
				}
			}
		}
	}
}
