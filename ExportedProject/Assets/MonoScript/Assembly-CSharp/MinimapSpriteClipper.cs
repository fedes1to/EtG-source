using System.Collections.Generic;
using UnityEngine;

public class MinimapSpriteClipper : MonoBehaviour
{
	private tk2dBaseSprite m_baseSprite;

	public void ForceUpdate()
	{
		ClipToTileBounds();
	}

	private void ClipToTileBounds()
	{
		Transform transform = base.transform;
		if (m_baseSprite == null)
		{
			m_baseSprite = GetComponent<tk2dBaseSprite>();
		}
		Bounds bounds = m_baseSprite.GetBounds();
		Vector2 vector = transform.position.XY() + bounds.min.XY();
		Vector2 vector2 = transform.position.XY() + bounds.max.XY();
		IntVector2 intVector = ((vector - Minimap.Instance.transform.position.XY()) * 8f).ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = ((vector2 - Minimap.Instance.transform.position.XY()) * 8f).ToIntVector2(VectorConversions.Floor);
		tk2dSpriteDefinition tk2dSpriteDefinition2 = m_baseSprite.Collection.spriteDefinitions[m_baseSprite.spriteId];
		Vector2 lhs = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 lhs2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < tk2dSpriteDefinition2.uvs.Length; i++)
		{
			lhs = Vector2.Min(lhs, tk2dSpriteDefinition2.uvs[i]);
			lhs2 = Vector2.Max(lhs2, tk2dSpriteDefinition2.uvs[i]);
		}
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector2> list3 = new List<Vector2>();
		for (int j = intVector.x; j <= intVector2.x; j++)
		{
			for (int k = intVector.y; k <= intVector2.y; k++)
			{
				if (Minimap.Instance[j, k])
				{
					int count = list.Count;
					float num = (float)j / 8f + Minimap.Instance.transform.position.x;
					float num2 = (float)k / 8f + Minimap.Instance.transform.position.y;
					float num3 = Mathf.Max(num, vector.x) - transform.position.x;
					float num4 = Mathf.Min(num + 0.125f, vector2.x) - transform.position.x;
					float num5 = Mathf.Max(num2, vector.y) - transform.position.y;
					float num6 = Mathf.Min(num2 + 0.125f, vector2.y) - transform.position.y;
					list.Add(new Vector3(num3, num5, 0f));
					list.Add(new Vector3(num4, num5, 0f));
					list.Add(new Vector3(num3, num6, 0f));
					list.Add(new Vector3(num4, num6, 0f));
					list2.Add(count);
					list2.Add(count + 2);
					list2.Add(count + 1);
					list2.Add(count + 2);
					list2.Add(count + 3);
					list2.Add(count + 1);
					float t = (num3 + transform.position.x - vector.x) / (vector2.x - vector.x);
					float t2 = (num4 + transform.position.x - vector.x) / (vector2.x - vector.x);
					float t3 = (num5 + transform.position.y - vector.y) / (vector2.y - vector.y);
					float t4 = (num6 + transform.position.y - vector.y) / (vector2.y - vector.y);
					float x = Mathf.Lerp(lhs.x, lhs2.x, t);
					float x2 = Mathf.Lerp(lhs.x, lhs2.x, t2);
					float y = Mathf.Lerp(lhs.y, lhs2.y, t3);
					float y2 = Mathf.Lerp(lhs.y, lhs2.y, t4);
					list3.Add(new Vector2(x, y));
					list3.Add(new Vector2(x2, y));
					list3.Add(new Vector2(x, y2));
					list3.Add(new Vector2(x2, y2));
				}
			}
		}
		MeshFilter component = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		component.mesh = mesh;
	}
}
