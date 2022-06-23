using System;
using UnityEngine;

public class CustomTrailRenderer : BraveBehaviour
{
	private class Point
	{
		public float timeCreated;

		public float fadeAlpha;

		public Vector3 position = Vector3.zero;

		public Quaternion rotation = Quaternion.identity;

		public float timeAlive
		{
			get
			{
				return BraveTime.ScaledTimeSinceStartup - timeCreated;
			}
		}

		public Point(Transform trans)
		{
			position = trans.position;
			rotation = trans.rotation;
			timeCreated = BraveTime.ScaledTimeSinceStartup;
		}

		public void Update(Transform trans)
		{
			position = trans.position;
			rotation = trans.rotation;
			timeCreated = BraveTime.ScaledTimeSinceStartup;
		}
	}

	public new SpeculativeRigidbody specRigidbody;

	public Material material;

	private Material instanceMaterial;

	public bool emit = true;

	private bool emittingDone;

	public float lifeTime = 1f;

	private float lifeTimeRatio = 1f;

	public Color[] colors;

	public float[] widths;

	public float maxAngle = 2f;

	public float minVertexDistance = 0.1f;

	public float maxVertexDistance = 1f;

	public float optimizeAngleInterval = 0.1f;

	public float optimizeDistanceInterval = 0.05f;

	public int optimizeCount = 30;

	private Mesh mesh;

	private Point[] points = new Point[100];

	private int numPoints;

	private float m_cachedMaxAngle;

	private float m_cachedMaxVertexDistance;

	private int m_cachedOptimizeCount;

	public void Awake()
	{
		m_cachedMaxAngle = maxAngle;
		m_cachedMaxVertexDistance = maxVertexDistance;
		m_cachedOptimizeCount = optimizeCount;
	}

	public void Start()
	{
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		if (!meshFilter)
		{
			return;
		}
		mesh = meshFilter.mesh;
		base.renderer = base.gameObject.AddComponent<MeshRenderer>();
		if ((bool)base.renderer)
		{
			instanceMaterial = new Material(material);
			base.renderer.material = instanceMaterial;
			if ((bool)specRigidbody && PhysicsEngine.HasInstance)
			{
				PhysicsEngine.Instance.OnPostRigidbodyMovement += OnPostRigidbodyMovement;
			}
		}
	}

	public void Update()
	{
		if (!specRigidbody)
		{
			UpdateMesh();
		}
	}

	private void OnPostRigidbodyMovement()
	{
		if ((bool)specRigidbody)
		{
			UpdateMesh();
		}
	}

	public void Reenable()
	{
		emit = true;
		emittingDone = false;
		if ((bool)base.renderer)
		{
			base.renderer.enabled = true;
		}
		maxAngle = m_cachedMaxAngle;
		maxVertexDistance = m_cachedMaxVertexDistance;
		optimizeCount = m_cachedOptimizeCount;
	}

	public void Clear()
	{
		for (int num = numPoints - 1; num >= 0; num--)
		{
			points[num] = null;
		}
		numPoints = 0;
		if ((bool)mesh)
		{
			mesh.Clear();
		}
	}

	private void UpdateMesh()
	{
		if ((bool)specRigidbody && specRigidbody.transform.rotation.eulerAngles.z != 0f)
		{
			base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - specRigidbody.transform.rotation.eulerAngles.z);
		}
		if (!emit)
		{
			emittingDone = true;
		}
		if (emittingDone)
		{
			emit = false;
		}
		int num = 0;
		for (int num2 = numPoints - 1; num2 >= 0; num2--)
		{
			Point point = points[num2];
			if (point != null && point.timeAlive < lifeTime)
			{
				break;
			}
			num++;
		}
		if (num > 1)
		{
			int num3 = numPoints - num + 1;
			while (numPoints > num3)
			{
				points[numPoints - 1] = null;
				numPoints--;
			}
		}
		if (numPoints > optimizeCount)
		{
			maxAngle += optimizeAngleInterval;
			maxVertexDistance += optimizeDistanceInterval;
			optimizeCount++;
		}
		if (emit)
		{
			if (numPoints == 0)
			{
				points[numPoints++] = new Point(base.transform);
				points[numPoints++] = new Point(base.transform);
			}
			if (numPoints == 1)
			{
				InsertPoint();
			}
			bool flag = false;
			float sqrMagnitude = (points[1].position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude > minVertexDistance * minVertexDistance)
			{
				if (sqrMagnitude > maxVertexDistance * maxVertexDistance)
				{
					flag = true;
				}
				else if (Quaternion.Angle(base.transform.rotation, points[1].rotation) > maxAngle)
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (numPoints == points.Length)
				{
					Array.Resize(ref points, points.Length + 50);
				}
				InsertPoint();
			}
			else
			{
				points[0].Update(base.transform);
			}
		}
		if (numPoints < 2)
		{
			base.renderer.enabled = false;
			return;
		}
		base.renderer.enabled = true;
		lifeTimeRatio = ((lifeTime != 0f) ? (1f / lifeTime) : 0f);
		if (!emit)
		{
			if (numPoints != 0)
			{
			}
			return;
		}
		Vector3[] array = new Vector3[numPoints * 2];
		Vector2[] array2 = new Vector2[numPoints * 2];
		int[] array3 = new int[(numPoints - 1) * 6];
		Color[] array4 = new Color[numPoints * 2];
		float num4 = 1f / (points[numPoints - 1].timeAlive - points[0].timeAlive);
		for (int i = 0; i < numPoints; i++)
		{
			Point point2 = points[i];
			float num5 = point2.timeAlive * lifeTimeRatio;
			Vector3 vector = ((i == 0 && numPoints > 1) ? (points[i + 1].position - points[i].position) : ((i == numPoints - 1 && numPoints > 1) ? (points[i].position - points[i - 1].position) : ((numPoints <= 2) ? Vector3.right : ((points[i + 1].position - points[i].position + (points[i].position - points[i - 1].position)) * 0.5f))));
			Color color;
			if (colors.Length == 0)
			{
				color = Color.Lerp(Color.white, Color.clear, num5);
			}
			else if (colors.Length == 1)
			{
				color = Color.Lerp(colors[0], Color.clear, num5);
			}
			else if (colors.Length == 2)
			{
				color = Color.Lerp(colors[0], colors[1], num5);
			}
			else if (num5 <= 0f)
			{
				color = colors[0];
			}
			else if (num5 >= 1f)
			{
				color = colors[colors.Length - 1];
			}
			else
			{
				float num6 = num5 * (float)(colors.Length - 1);
				int num7 = Mathf.Min(colors.Length - 2, (int)Mathf.Floor(num6));
				float num8 = Mathf.InverseLerp(num7, num7 + 1, num6);
				if (num7 < 0 || num7 + 1 >= colors.Length)
				{
					Debug.LogFormat("{0}, {1}, {2}, {3}", num6, num7, num8, num7 + 1);
				}
				color = Color.Lerp(colors[num7], colors[num7 + 1], num8);
			}
			array4[i * 2] = color;
			array4[i * 2 + 1] = color;
			Vector3 vector2 = point2.position;
			if (i > 0 && i == numPoints - 1)
			{
				float t = Mathf.InverseLerp(points[i - 1].timeAlive, point2.timeAlive, lifeTime);
				vector2 = Vector3.Lerp(points[i - 1].position, point2.position, t);
			}
			float num9;
			if (widths.Length == 0)
			{
				num9 = 1f;
			}
			else if (widths.Length == 1)
			{
				num9 = widths[0];
			}
			else if (widths.Length == 2)
			{
				num9 = Mathf.Lerp(widths[0], widths[1], num5);
			}
			else if (num5 <= 0f)
			{
				num9 = widths[0];
			}
			else if (num5 >= 1f)
			{
				num9 = widths[widths.Length - 1];
			}
			else
			{
				float num10 = num5 * (float)(widths.Length - 1);
				int num11 = (int)Mathf.Floor(num10);
				float t2 = Mathf.InverseLerp(num11, num11 + 1, num10);
				num9 = Mathf.Lerp(widths[num11], widths[num11 + 1], t2);
			}
			vector = vector.normalized.RotateBy(Quaternion.Euler(0f, 0f, 90f)) * 0.5f * num9;
			array[i * 2] = vector2 - base.transform.position + vector;
			array[i * 2 + 1] = vector2 - base.transform.position - vector;
			float x = (point2.timeAlive - points[0].timeAlive) * num4;
			array2[i * 2] = new Vector2(x, 0f);
			array2[i * 2 + 1] = new Vector2(x, 1f);
			if (i > 0)
			{
				int num12 = (i - 1) * 6;
				int num13 = i * 2;
				array3[num12] = num13 - 2;
				array3[num12 + 1] = num13 - 1;
				array3[num12 + 2] = num13;
				array3[num12 + 3] = num13 + 1;
				array3[num12 + 4] = num13;
				array3[num12 + 5] = num13 - 1;
			}
		}
		mesh.Clear();
		mesh.vertices = array;
		mesh.colors = array4;
		mesh.uv = array2;
		mesh.triangles = array3;
	}

	private void InsertPoint()
	{
		for (int num = numPoints; num > 0; num--)
		{
			points[num] = points[num - 1];
		}
		points[0] = new Point(base.transform);
		numPoints++;
	}
}
