using System;
using UnityEngine;

[Serializable]
[dfTooltip("Implements a Sprite that can be tiled horizontally and vertically")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_tiled_sprite.html")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[AddComponentMenu("Daikon Forge/User Interface/Sprite/Tiled")]
public class dfTiledSprite : dfSprite
{
	private static int[] quadTriangles = new int[6] { 0, 1, 3, 3, 1, 2 };

	private static Vector2[] quadUV = new Vector2[4];

	[SerializeField]
	protected Vector2 tileScale = Vector2.one;

	[SerializeField]
	protected Vector2 tileScroll = Vector2.zero;

	public bool EnableBlackLineFix;

	public Vector2 TileScale
	{
		get
		{
			return tileScale;
		}
		set
		{
			if (Vector2.Distance(value, tileScale) > float.Epsilon)
			{
				tileScale = Vector2.Max(Vector2.one * 0.1f, value);
				Invalidate();
			}
		}
	}

	public Vector2 TileScroll
	{
		get
		{
			return tileScroll;
		}
		set
		{
			if (Vector2.Distance(value, tileScroll) > float.Epsilon)
			{
				tileScroll = value;
				Invalidate();
			}
		}
	}

	protected override void OnRebuildRenderData()
	{
		if (base.Atlas == null)
		{
			return;
		}
		dfAtlas.ItemInfo spriteInfo = base.SpriteInfo;
		if (spriteInfo == null)
		{
			return;
		}
		renderData.Material = base.Atlas.Material;
		dfList<Vector3> vertices = renderData.Vertices;
		dfList<Vector2> uV = renderData.UV;
		dfList<Color32> colors = renderData.Colors;
		dfList<int> triangles = renderData.Triangles;
		Vector2[] spriteUV = buildQuadUV();
		Vector2 vector = Vector2.Scale(spriteInfo.sizeInPixels, tileScale);
		Vector2 vector2 = new Vector2(tileScroll.x % 1f, tileScroll.y % 1f);
		float num = ((!EnableBlackLineFix) ? 0f : (-0.1f));
		for (float num2 = 0f - Mathf.Abs(vector2.y * vector.y); num2 < size.y; num2 += vector.y)
		{
			for (float num3 = 0f - Mathf.Abs(vector2.x * vector.x); num3 < size.x; num3 += vector.x)
			{
				int count = vertices.Count;
				vertices.Add(new Vector3(num3, 0f - num2));
				vertices.Add(new Vector3(num3 + vector.x, 0f - num2));
				vertices.Add(new Vector3(num3 + vector.x, 0f - num2 + (0f - vector.y) + num));
				vertices.Add(new Vector3(num3, 0f - num2 + (0f - vector.y) + num));
				addQuadTriangles(triangles, count);
				addQuadUV(uV, spriteUV);
				addQuadColors(colors);
			}
		}
		clipQuads(vertices, uV);
		float num4 = PixelsToUnits();
		Vector3 vector3 = pivot.TransformToUpperLeft(size);
		for (int i = 0; i < vertices.Count; i++)
		{
			vertices[i] = (vertices[i] + vector3) * num4;
		}
	}

	private void clipQuads(dfList<Vector3> verts, dfList<Vector2> uv)
	{
		float num = 0f;
		float num2 = size.x;
		float num3 = 0f - size.y;
		float num4 = 0f;
		if (fillAmount < 1f)
		{
			if (fillDirection == dfFillDirection.Horizontal)
			{
				if (!invertFill)
				{
					num2 = size.x * fillAmount;
				}
				else
				{
					num = size.x - size.x * fillAmount;
				}
			}
			else if (!invertFill)
			{
				num3 = (0f - size.y) * fillAmount;
			}
			else
			{
				num4 = (0f - size.y) * (1f - fillAmount);
			}
		}
		for (int i = 0; i < verts.Count; i += 4)
		{
			Vector3 vector = verts[i];
			Vector3 vector2 = verts[i + 1];
			Vector3 vector3 = verts[i + 2];
			Vector3 vector4 = verts[i + 3];
			float num5 = vector2.x - vector.x;
			float num6 = vector.y - vector4.y;
			if (vector.x < num)
			{
				float t = (num - vector.x) / num5;
				vector = (verts[i] = new Vector3(Mathf.Max(num, vector.x), vector.y, vector.z));
				vector2 = (verts[i + 1] = new Vector3(Mathf.Max(num, vector2.x), vector2.y, vector2.z));
				vector3 = (verts[i + 2] = new Vector3(Mathf.Max(num, vector3.x), vector3.y, vector3.z));
				vector4 = (verts[i + 3] = new Vector3(Mathf.Max(num, vector4.x), vector4.y, vector4.z));
				float x = Mathf.Lerp(uv[i].x, uv[i + 1].x, t);
				uv[i] = new Vector2(x, uv[i].y);
				uv[i + 3] = new Vector2(x, uv[i + 3].y);
				num5 = vector2.x - vector.x;
			}
			if (vector2.x > num2)
			{
				float t2 = 1f - (num2 - vector2.x + num5) / num5;
				vector = (verts[i] = new Vector3(Mathf.Min(vector.x, num2), vector.y, vector.z));
				vector2 = (verts[i + 1] = new Vector3(Mathf.Min(vector2.x, num2), vector2.y, vector2.z));
				vector3 = (verts[i + 2] = new Vector3(Mathf.Min(vector3.x, num2), vector3.y, vector3.z));
				vector4 = (verts[i + 3] = new Vector3(Mathf.Min(vector4.x, num2), vector4.y, vector4.z));
				float x2 = Mathf.Lerp(uv[i + 1].x, uv[i].x, t2);
				uv[i + 1] = new Vector2(x2, uv[i + 1].y);
				uv[i + 2] = new Vector2(x2, uv[i + 2].y);
				num5 = vector2.x - vector.x;
			}
			if (vector4.y < num3)
			{
				float t3 = 1f - Mathf.Abs(0f - num3 + vector.y) / num6;
				vector = (verts[i] = new Vector3(vector.x, Mathf.Max(vector.y, num3), vector2.z));
				vector2 = (verts[i + 1] = new Vector3(vector2.x, Mathf.Max(vector2.y, num3), vector2.z));
				vector3 = (verts[i + 2] = new Vector3(vector3.x, Mathf.Max(vector3.y, num3), vector3.z));
				vector4 = (verts[i + 3] = new Vector3(vector4.x, Mathf.Max(vector4.y, num3), vector4.z));
				float y = Mathf.Lerp(uv[i + 3].y, uv[i].y, t3);
				uv[i + 3] = new Vector2(uv[i + 3].x, y);
				uv[i + 2] = new Vector2(uv[i + 2].x, y);
				num6 = Mathf.Abs(vector4.y - vector.y);
			}
			if (vector.y > num4)
			{
				float t4 = Mathf.Abs(num4 - vector.y) / num6;
				vector = (verts[i] = new Vector3(vector.x, Mathf.Min(num4, vector.y), vector.z));
				vector2 = (verts[i + 1] = new Vector3(vector2.x, Mathf.Min(num4, vector2.y), vector2.z));
				vector3 = (verts[i + 2] = new Vector3(vector3.x, Mathf.Min(num4, vector3.y), vector3.z));
				vector4 = (verts[i + 3] = new Vector3(vector4.x, Mathf.Min(num4, vector4.y), vector4.z));
				float y2 = Mathf.Lerp(uv[i].y, uv[i + 3].y, t4);
				uv[i] = new Vector2(uv[i].x, y2);
				uv[i + 1] = new Vector2(uv[i + 1].x, y2);
			}
		}
	}

	private void addQuadTriangles(dfList<int> triangles, int baseIndex)
	{
		for (int i = 0; i < quadTriangles.Length; i++)
		{
			triangles.Add(quadTriangles[i] + baseIndex);
		}
	}

	private void addQuadColors(dfList<Color32> colors)
	{
		colors.EnsureCapacity(colors.Count + 4);
		Color32 item = ApplyOpacity((!base.IsEnabled) ? disabledColor : color);
		for (int i = 0; i < 4; i++)
		{
			colors.Add(item);
		}
	}

	private void addQuadUV(dfList<Vector2> uv, Vector2[] spriteUV)
	{
		uv.AddRange(spriteUV);
	}

	private Vector2[] buildQuadUV()
	{
		dfAtlas.ItemInfo spriteInfo = base.SpriteInfo;
		Rect region = spriteInfo.region;
		quadUV[0] = new Vector2(region.x, region.yMax);
		quadUV[1] = new Vector2(region.xMax, region.yMax);
		quadUV[2] = new Vector2(region.xMax, region.y);
		quadUV[3] = new Vector2(region.x, region.y);
		Vector2 zero = Vector2.zero;
		if (flip.IsSet(dfSpriteFlip.FlipHorizontal))
		{
			zero = quadUV[1];
			quadUV[1] = quadUV[0];
			quadUV[0] = zero;
			zero = quadUV[3];
			quadUV[3] = quadUV[2];
			quadUV[2] = zero;
		}
		if (flip.IsSet(dfSpriteFlip.FlipVertical))
		{
			zero = quadUV[0];
			quadUV[0] = quadUV[3];
			quadUV[3] = zero;
			zero = quadUV[1];
			quadUV[1] = quadUV[2];
			quadUV[2] = zero;
		}
		return quadUV;
	}
}
