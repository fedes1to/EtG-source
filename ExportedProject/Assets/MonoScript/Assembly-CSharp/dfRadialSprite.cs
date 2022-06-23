using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[dfTooltip("Implements a sprite that can be filled in a radial fashion instead of strictly horizontally or vertically like other sprite classes. Useful for spell cooldown effects, map effects, etc.")]
[dfCategory("Basic Controls")]
[AddComponentMenu("Daikon Forge/User Interface/Sprite/Radial")]
[ExecuteInEditMode]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_radial_sprite.html")]
public class dfRadialSprite : dfSprite
{
	private static Vector3[] baseVerts = new Vector3[8]
	{
		new Vector3(0f, 0.5f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(0.5f, 0f, 0f),
		new Vector3(0.5f, -0.5f, 0f),
		new Vector3(0f, -0.5f, 0f),
		new Vector3(-0.5f, -0.5f, 0f),
		new Vector3(-0.5f, 0f, 0f),
		new Vector3(-0.5f, 0.5f, 0f)
	};

	[SerializeField]
	protected dfPivotPoint fillOrigin = dfPivotPoint.MiddleCenter;

	public dfPivotPoint FillOrigin
	{
		get
		{
			return fillOrigin;
		}
		set
		{
			if (value != fillOrigin)
			{
				fillOrigin = value;
				Invalidate();
			}
		}
	}

	protected override void OnRebuildRenderData()
	{
		if (!(base.Atlas == null))
		{
			dfAtlas.ItemInfo spriteInfo = base.SpriteInfo;
			if (!(spriteInfo == null))
			{
				renderData.Material = base.Atlas.Material;
				List<Vector3> verts = null;
				List<int> indices = null;
				List<Vector2> uv = null;
				buildMeshData(ref verts, ref indices, ref uv);
				Color32[] list = buildColors(verts.Count);
				renderData.Vertices.AddRange(verts);
				renderData.Triangles.AddRange(indices);
				renderData.UV.AddRange(uv);
				renderData.Colors.AddRange(list);
			}
		}
	}

	private void buildMeshData(ref List<Vector3> verts, ref List<int> indices, ref List<Vector2> uv)
	{
		List<Vector3> list = (verts = new List<Vector3>());
		verts.AddRange(baseVerts);
		int num = 8;
		int num2 = -1;
		switch (fillOrigin)
		{
		case dfPivotPoint.TopLeft:
			num = 4;
			num2 = 5;
			list.RemoveAt(6);
			list.RemoveAt(0);
			break;
		case dfPivotPoint.TopCenter:
			num = 6;
			num2 = 0;
			break;
		case dfPivotPoint.TopRight:
			num = 4;
			num2 = 0;
			list.RemoveAt(2);
			list.RemoveAt(0);
			break;
		case dfPivotPoint.MiddleLeft:
			num = 6;
			num2 = 6;
			break;
		case dfPivotPoint.MiddleRight:
			num = 6;
			num2 = 2;
			break;
		case dfPivotPoint.BottomLeft:
			num = 4;
			num2 = 4;
			list.RemoveAt(6);
			list.RemoveAt(4);
			break;
		case dfPivotPoint.BottomCenter:
			num = 6;
			num2 = 4;
			break;
		case dfPivotPoint.BottomRight:
			num = 4;
			num2 = 2;
			list.RemoveAt(4);
			list.RemoveAt(2);
			break;
		case dfPivotPoint.MiddleCenter:
			num = 8;
			list.Add(list[0]);
			list.Insert(0, Vector3.zero);
			num2 = 0;
			break;
		default:
			throw new NotImplementedException();
		}
		makeFirst(list, num2);
		List<int> list2 = (indices = buildTriangles(list));
		float num3 = 1f / (float)num;
		float num4 = fillAmount.Quantize(num3);
		int num5 = Mathf.CeilToInt(num4 / num3) + 1;
		for (int i = num5; i < num; i++)
		{
			if (invertFill)
			{
				list2.RemoveRange(0, 3);
				continue;
			}
			list.RemoveAt(list.Count - 1);
			list2.RemoveRange(list2.Count - 3, 3);
		}
		if (fillAmount < 1f)
		{
			int index = list2[(!invertFill) ? (list2.Count - 2) : 2];
			int index2 = list2[invertFill ? 1 : (list2.Count - 1)];
			float t = (base.FillAmount - num4) / num3;
			list[index2] = Vector3.Lerp(list[index], list[index2], t);
		}
		uv = buildUV(list);
		float num6 = PixelsToUnits();
		Vector2 vector = num6 * size;
		Vector3 vector2 = pivot.TransformToCenter(size) * num6;
		for (int j = 0; j < list.Count; j++)
		{
			list[j] = Vector3.Scale(list[j], vector) + vector2;
		}
	}

	private void makeFirst(List<Vector3> list, int index)
	{
		if (index != 0)
		{
			List<Vector3> range = list.GetRange(index, list.Count - index);
			list.RemoveRange(index, list.Count - index);
			list.InsertRange(0, range);
		}
	}

	private List<int> buildTriangles(List<Vector3> verts)
	{
		List<int> list = new List<int>();
		int count = verts.Count;
		for (int i = 1; i < count - 1; i++)
		{
			list.Add(0);
			list.Add(i);
			list.Add(i + 1);
		}
		return list;
	}

	private List<Vector2> buildUV(List<Vector3> verts)
	{
		dfAtlas.ItemInfo spriteInfo = base.SpriteInfo;
		if (spriteInfo == null)
		{
			return null;
		}
		Rect rect = spriteInfo.region;
		if (flip.IsSet(dfSpriteFlip.FlipHorizontal))
		{
			rect = new Rect(rect.xMax, rect.y, 0f - rect.width, rect.height);
		}
		if (flip.IsSet(dfSpriteFlip.FlipVertical))
		{
			rect = new Rect(rect.x, rect.yMax, rect.width, 0f - rect.height);
		}
		Vector2 vector = new Vector2(rect.x, rect.y);
		Vector2 vector2 = new Vector2(0.5f, 0.5f);
		Vector2 b = new Vector2(rect.width, rect.height);
		List<Vector2> list = new List<Vector2>(verts.Count);
		for (int i = 0; i < verts.Count; i++)
		{
			Vector2 a = (Vector2)verts[i] + vector2;
			list.Add(Vector2.Scale(a, b) + vector);
		}
		return list;
	}

	private Color32[] buildColors(int vertCount)
	{
		Color32 color = ApplyOpacity((!base.IsEnabled) ? disabledColor : base.color);
		Color32[] array = new Color32[vertCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = color;
		}
		return array;
	}
}
