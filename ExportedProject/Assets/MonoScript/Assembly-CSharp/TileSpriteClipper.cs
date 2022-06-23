using Dungeonator;
using UnityEngine;

public class TileSpriteClipper : BraveBehaviour
{
	public enum ClipMode
	{
		GroundDecal,
		WallEnterer,
		PitBounds,
		ClipBelowY
	}

	public bool doOptimize = true;

	public bool updateEveryFrame;

	public ClipMode clipMode;

	[ShowInInspectorIf("clipMode", 3, false)]
	public float clipY;

	private Vector3[] m_vertices;

	private int[] m_triangles;

	private Vector2[] m_uvs;

	private void Start()
	{
		DoClip();
	}

	private void OnEnable()
	{
		Start();
	}

	private void OnDisable()
	{
		if ((bool)base.sprite)
		{
			base.sprite.ForceBuild();
		}
	}

	private void LateUpdate()
	{
		if (updateEveryFrame)
		{
			DoClip();
		}
		if ((bool)base.sprite && !base.sprite.attachParent)
		{
			base.sprite.UpdateZDepth();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void DoClip()
	{
		if (clipMode == ClipMode.ClipBelowY)
		{
			ClipToY();
		}
		else
		{
			ClipToTileBounds();
		}
	}

	private void ClipToY()
	{
		if (BraveUtility.isLoadingLevel || GameManager.Instance.IsLoadingLevel || !base.sprite)
		{
			return;
		}
		Transform transform = base.transform;
		Bounds bounds = base.sprite.GetBounds();
		Vector2 b = bounds.min.XY();
		Vector2 b2 = bounds.max.XY();
		Vector3 vector = new Vector3(Mathf.Sign(transform.lossyScale.x), Mathf.Sign(transform.lossyScale.y), Mathf.Sign(transform.lossyScale.z));
		Vector2 lhs = Vector2.Scale(vector.XY(), b);
		Vector2 rhs = Vector2.Scale(vector.XY(), b2);
		b = transform.position.XY() + Vector2.Min(lhs, rhs);
		b2 = transform.position.XY() + Vector2.Max(lhs, rhs);
		tk2dSpriteDefinition tk2dSpriteDefinition2 = base.sprite.Collection.spriteDefinitions[base.sprite.spriteId];
		Vector2 lhs2 = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 lhs3 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < tk2dSpriteDefinition2.uvs.Length; i++)
		{
			lhs2 = Vector2.Min(lhs2, tk2dSpriteDefinition2.uvs[i]);
			lhs3 = Vector2.Max(lhs3, tk2dSpriteDefinition2.uvs[i]);
		}
		if (m_vertices == null || m_vertices.Length != 4)
		{
			m_vertices = new Vector3[4];
		}
		if (m_triangles == null || m_triangles.Length != 6)
		{
			m_triangles = new int[6];
		}
		if (m_uvs == null || m_uvs.Length != 4)
		{
			m_uvs = new Vector2[4];
		}
		float num = b.x - transform.position.x;
		float num2 = b2.x - transform.position.x;
		float num3 = Mathf.Max(b.y, Mathf.Min(clipY, b2.y)) - transform.position.y;
		float num4 = b2.y - transform.position.y;
		Vector3 b3 = new Vector3(num, num3, 0f);
		Vector3 b4 = new Vector3(num2, num3, 0f);
		Vector3 b5 = new Vector3(num, num4, 0f);
		Vector3 b6 = new Vector3(num2, num4, 0f);
		b3 = Vector3.Scale(vector, b3);
		b4 = Vector3.Scale(vector, b4);
		b5 = Vector3.Scale(vector, b5);
		b6 = Vector3.Scale(vector, b6);
		m_vertices[0] = b3;
		m_vertices[1] = b4;
		m_vertices[2] = b5;
		m_vertices[3] = b6;
		if (base.sprite.ShouldDoTilt)
		{
			for (int j = 0; j < 4; j++)
			{
				if (base.sprite.IsPerpendicular)
				{
					m_vertices[j] = m_vertices[j].WithZ(m_vertices[j].z - m_vertices[j].y);
				}
				else
				{
					m_vertices[j] = m_vertices[j].WithZ(m_vertices[j].z + m_vertices[j].y);
				}
			}
		}
		m_triangles[0] = 0;
		m_triangles[1] = 2;
		m_triangles[2] = 1;
		m_triangles[3] = 2;
		m_triangles[4] = 3;
		m_triangles[5] = 1;
		float t = (num + transform.position.x - b.x) / (b2.x - b.x);
		float t2 = (num2 + transform.position.x - b.x) / (b2.x - b.x);
		float t3 = (num3 + transform.position.y - b.y) / (b2.y - b.y);
		float t4 = (num4 + transform.position.y - b.y) / (b2.y - b.y);
		if (tk2dSpriteDefinition2.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
		{
			Vector2 vector2 = new Vector2(Mathf.Lerp(lhs2.x, lhs3.x, t3), Mathf.Lerp(lhs2.y, lhs3.y, t));
			Vector2 vector3 = new Vector2(Mathf.Lerp(lhs2.x, lhs3.x, t4), Mathf.Lerp(lhs2.y, lhs3.y, t2));
			m_uvs[0] = new Vector2(vector2.x, vector2.y);
			m_uvs[1] = new Vector2(vector2.x, vector3.y);
			m_uvs[2] = new Vector2(vector3.x, vector2.y);
			m_uvs[3] = new Vector2(vector3.x, vector3.y);
		}
		else
		{
			float x = Mathf.Lerp(lhs2.x, lhs3.x, t);
			float x2 = Mathf.Lerp(lhs2.x, lhs3.x, t2);
			float y = Mathf.Lerp(lhs2.y, lhs3.y, t3);
			float y2 = Mathf.Lerp(lhs2.y, lhs3.y, t4);
			m_uvs[0] = new Vector2(x, y);
			m_uvs[1] = new Vector2(x2, y);
			m_uvs[2] = new Vector2(x, y2);
			m_uvs[3] = new Vector2(x2, y2);
		}
		MeshFilter component = GetComponent<MeshFilter>();
		Mesh mesh = component.mesh;
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else if (mesh.vertexCount != m_vertices.Length)
		{
			mesh.Clear(false);
		}
		mesh.vertices = m_vertices;
		mesh.triangles = m_triangles;
		mesh.uv = m_uvs;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		component.mesh = mesh;
	}

	private void ClipToTileBounds()
	{
		if (BraveUtility.isLoadingLevel || GameManager.Instance.IsLoadingLevel || !base.sprite)
		{
			return;
		}
		Transform transform = base.transform;
		Bounds bounds = base.sprite.GetBounds();
		Vector2 b = bounds.min.XY();
		Vector2 b2 = bounds.max.XY();
		Vector3 vector = new Vector3(Mathf.Sign(transform.lossyScale.x), Mathf.Sign(transform.lossyScale.y), Mathf.Sign(transform.lossyScale.z));
		Vector2 lhs = Vector2.Scale(vector.XY(), b);
		Vector2 rhs = Vector2.Scale(vector.XY(), b2);
		b = transform.position.XY() + Vector2.Min(lhs, rhs);
		b2 = transform.position.XY() + Vector2.Max(lhs, rhs);
		IntVector2 intVector = b.ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = b2.ToIntVector2(VectorConversions.Floor);
		tk2dSpriteDefinition tk2dSpriteDefinition2 = base.sprite.Collection.spriteDefinitions[base.sprite.spriteId];
		Vector2 lhs2 = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 lhs3 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < tk2dSpriteDefinition2.uvs.Length; i++)
		{
			lhs2 = Vector2.Min(lhs2, tk2dSpriteDefinition2.uvs[i]);
			lhs3 = Vector2.Max(lhs3, tk2dSpriteDefinition2.uvs[i]);
		}
		int num = (intVector2.x - intVector.x + 2) * (intVector2.y - intVector.y + 2) * 4 * 2;
		if (m_vertices != null && num / 2 > m_vertices.Length)
		{
			m_vertices = null;
			m_triangles = null;
			m_uvs = null;
		}
		if (m_vertices == null)
		{
			m_vertices = new Vector3[num];
		}
		if (m_triangles == null)
		{
			m_triangles = new int[m_vertices.Length / 4 * 6];
		}
		if (m_uvs == null)
		{
			m_uvs = new Vector2[num];
		}
		int num2 = 0;
		int num3 = 0;
		for (int j = intVector.x; j <= intVector2.x; j++)
		{
			for (int k = intVector.y; k <= intVector2.y; k++)
			{
				if (j < 0 || j >= GameManager.Instance.Dungeon.data.Width || k < 0 || k >= GameManager.Instance.Dungeon.data.Height)
				{
					continue;
				}
				CellData cellData = GameManager.Instance.Dungeon.data.cellData[j][k];
				if (cellData == null)
				{
					continue;
				}
				if (clipMode == ClipMode.GroundDecal)
				{
					if ((cellData.type != CellType.FLOOR && !cellData.fallingPrevented) || cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Water)
					{
						continue;
					}
				}
				else if (clipMode == ClipMode.WallEnterer)
				{
					if (cellData.type == CellType.WALL && !GameManager.Instance.Dungeon.data.isFaceWallLower(j, k))
					{
						continue;
					}
				}
				else if (clipMode == ClipMode.PitBounds && (cellData.type != CellType.PIT || cellData.fallingPrevented))
				{
					continue;
				}
				int num4 = num2;
				float num5 = Mathf.Max(j, b.x) - transform.position.x;
				float num6 = Mathf.Min(j + 1, b2.x) - transform.position.x;
				float num7 = Mathf.Max(k, b.y) - transform.position.y;
				float num8 = Mathf.Min(k + 1, b2.y) - transform.position.y;
				Vector3 b3 = new Vector3(num5, num7, 0f);
				Vector3 b4 = new Vector3(num6, num7, 0f);
				Vector3 b5 = new Vector3(num5, num8, 0f);
				Vector3 b6 = new Vector3(num6, num8, 0f);
				b3 = Vector3.Scale(vector, b3);
				b4 = Vector3.Scale(vector, b4);
				b5 = Vector3.Scale(vector, b5);
				b6 = Vector3.Scale(vector, b6);
				m_vertices[num2] = b3;
				m_vertices[num2 + 1] = b4;
				m_vertices[num2 + 2] = b5;
				m_vertices[num2 + 3] = b6;
				if (base.sprite.ShouldDoTilt)
				{
					for (int l = num2; l < num2 + 4; l++)
					{
						if (base.sprite.IsPerpendicular)
						{
							m_vertices[l] = m_vertices[l].WithZ(m_vertices[l].z - m_vertices[l].y);
						}
						else
						{
							m_vertices[l] = m_vertices[l].WithZ(m_vertices[l].z + m_vertices[l].y);
						}
					}
				}
				m_triangles[num3] = num4;
				m_triangles[num3 + 1] = num4 + 2;
				m_triangles[num3 + 2] = num4 + 1;
				m_triangles[num3 + 3] = num4 + 2;
				m_triangles[num3 + 4] = num4 + 3;
				m_triangles[num3 + 5] = num4 + 1;
				float t = (num5 + transform.position.x - b.x) / (b2.x - b.x);
				float t2 = (num6 + transform.position.x - b.x) / (b2.x - b.x);
				float t3 = (num7 + transform.position.y - b.y) / (b2.y - b.y);
				float t4 = (num8 + transform.position.y - b.y) / (b2.y - b.y);
				if (tk2dSpriteDefinition2.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
				{
					Vector2 vector2 = new Vector2(Mathf.Lerp(lhs2.x, lhs3.x, t3), Mathf.Lerp(lhs2.y, lhs3.y, t));
					Vector2 vector3 = new Vector2(Mathf.Lerp(lhs2.x, lhs3.x, t4), Mathf.Lerp(lhs2.y, lhs3.y, t2));
					m_uvs[num2] = new Vector2(vector2.x, vector2.y);
					m_uvs[num2 + 1] = new Vector2(vector2.x, vector3.y);
					m_uvs[num2 + 2] = new Vector2(vector3.x, vector2.y);
					m_uvs[num2 + 3] = new Vector2(vector3.x, vector3.y);
				}
				else
				{
					float x = Mathf.Lerp(lhs2.x, lhs3.x, t);
					float x2 = Mathf.Lerp(lhs2.x, lhs3.x, t2);
					float y = Mathf.Lerp(lhs2.y, lhs3.y, t3);
					float y2 = Mathf.Lerp(lhs2.y, lhs3.y, t4);
					m_uvs[num2] = new Vector2(x, y);
					m_uvs[num2 + 1] = new Vector2(x2, y);
					m_uvs[num2 + 2] = new Vector2(x, y2);
					m_uvs[num2 + 3] = new Vector2(x2, y2);
				}
				num2 += 4;
				num3 += 6;
			}
		}
		for (int m = num2; m < m_vertices.Length; m++)
		{
			m_vertices[m] = Vector3.zero;
			m_uvs[m] = Vector2.zero;
		}
		for (int n = num3; n < m_triangles.Length; n++)
		{
			m_triangles[n] = 0;
		}
		MeshFilter component = GetComponent<MeshFilter>();
		Mesh mesh = component.mesh;
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		mesh.vertices = m_vertices;
		mesh.triangles = m_triangles;
		mesh.uv = m_uvs;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		component.mesh = mesh;
	}
}
