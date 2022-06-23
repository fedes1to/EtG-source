using System;
using UnityEngine;

[Serializable]
[dfTooltip("Displays a sprite from a Texture Atlas using 9-slice scaling")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[AddComponentMenu("Daikon Forge/User Interface/Sprite/Sliced")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_sliced_sprite.html")]
public class dfSlicedSprite : dfSprite
{
	private static int[] triangleIndices = new int[54]
	{
		0, 1, 2, 2, 3, 0, 4, 5, 6, 6,
		7, 4, 8, 9, 10, 10, 11, 8, 12, 13,
		14, 14, 15, 12, 1, 4, 7, 7, 2, 1,
		9, 12, 15, 15, 10, 9, 3, 2, 9, 9,
		8, 3, 7, 6, 13, 13, 12, 7, 2, 7,
		12, 12, 9, 2
	};

	private static int[][] horzFill = new int[4][]
	{
		new int[4] { 0, 1, 4, 5 },
		new int[4] { 3, 2, 7, 6 },
		new int[4] { 8, 9, 12, 13 },
		new int[4] { 11, 10, 15, 14 }
	};

	private static int[][] vertFill = new int[4][]
	{
		new int[4] { 11, 8, 3, 0 },
		new int[4] { 10, 9, 2, 1 },
		new int[4] { 15, 12, 7, 4 },
		new int[4] { 14, 13, 6, 5 }
	};

	private static int[][] fillIndices = new int[4][]
	{
		new int[4],
		new int[4],
		new int[4],
		new int[4]
	};

	private static Vector3[] verts = new Vector3[16];

	private static Vector2[] uv = new Vector2[16];

	protected override void OnRebuildRenderData()
	{
		if (base.Atlas == null)
		{
			return;
		}
		dfAtlas.ItemInfo spriteInfo = base.SpriteInfo;
		if (!(spriteInfo == null))
		{
			renderData.Material = base.Atlas.Material;
			if (spriteInfo.border.horizontal == 0 && spriteInfo.border.vertical == 0)
			{
				base.OnRebuildRenderData();
				return;
			}
			Color32 color = ApplyOpacity((!base.IsEnabled) ? disabledColor : base.color);
			RenderOptions renderOptions = default(RenderOptions);
			renderOptions.atlas = atlas;
			renderOptions.color = color;
			renderOptions.fillAmount = fillAmount;
			renderOptions.fillDirection = fillDirection;
			renderOptions.flip = flip;
			renderOptions.invertFill = invertFill;
			renderOptions.offset = pivot.TransformToUpperLeft(base.Size);
			renderOptions.pixelsToUnits = PixelsToUnits();
			renderOptions.size = base.Size;
			renderOptions.spriteInfo = base.SpriteInfo;
			RenderOptions options = renderOptions;
			renderSprite(renderData, options);
		}
	}

	internal new static void renderSprite(dfRenderData renderData, RenderOptions options)
	{
		if (!(options.fillAmount <= float.Epsilon))
		{
			options.baseIndex = renderData.Vertices.Count;
			rebuildTriangles(renderData, options);
			rebuildVertices(renderData, options);
			rebuildUV(renderData, options);
			rebuildColors(renderData, options);
			if (options.fillAmount < 1f)
			{
				doFill(renderData, options);
			}
		}
	}

	private static void rebuildTriangles(dfRenderData renderData, RenderOptions options)
	{
		int baseIndex = options.baseIndex;
		dfList<int> triangles = renderData.Triangles;
		for (int i = 0; i < triangleIndices.Length; i++)
		{
			triangles.Add(baseIndex + triangleIndices[i]);
		}
	}

	private static void doFill(dfRenderData renderData, RenderOptions options)
	{
		int baseIndex = options.baseIndex;
		dfList<Vector3> vertices = renderData.Vertices;
		dfList<Vector2> uV = renderData.UV;
		int[][] array = getFillIndices(options.fillDirection, baseIndex);
		bool flag = options.invertFill;
		if (options.fillDirection == dfFillDirection.Vertical)
		{
			flag = !flag;
		}
		if (flag)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Array.Reverse(array[i]);
			}
		}
		int index = ((options.fillDirection != 0) ? 1 : 0);
		float num = vertices[array[0][flag ? 3 : 0]][index];
		float num2 = vertices[array[0][(!flag) ? 3 : 0]][index];
		float num3 = Mathf.Abs(num2 - num);
		float num4 = (flag ? (num2 - options.fillAmount * num3) : (num + options.fillAmount * num3));
		for (int j = 0; j < array.Length; j++)
		{
			if (!flag)
			{
				for (int num5 = 3; num5 > 0; num5--)
				{
					float num6 = vertices[array[j][num5]][index];
					if (!(num6 < num4))
					{
						Vector3 value = vertices[array[j][num5]];
						value[index] = num4;
						vertices[array[j][num5]] = value;
						float num7 = vertices[array[j][num5 - 1]][index];
						if (!(num7 > num4))
						{
							float num8 = num6 - num7;
							float t = (num4 - num7) / num8;
							float b = uV[array[j][num5]][index];
							float a = uV[array[j][num5 - 1]][index];
							Vector2 value2 = uV[array[j][num5]];
							value2[index] = Mathf.Lerp(a, b, t);
							uV[array[j][num5]] = value2;
						}
					}
				}
				continue;
			}
			for (int k = 1; k < 4; k++)
			{
				float num9 = vertices[array[j][k]][index];
				if (!(num9 > num4))
				{
					Vector3 value3 = vertices[array[j][k]];
					value3[index] = num4;
					vertices[array[j][k]] = value3;
					float num10 = vertices[array[j][k - 1]][index];
					if (!(num10 < num4))
					{
						float num11 = num9 - num10;
						float t2 = (num4 - num10) / num11;
						float b2 = uV[array[j][k]][index];
						float a2 = uV[array[j][k - 1]][index];
						Vector2 value4 = uV[array[j][k]];
						value4[index] = Mathf.Lerp(a2, b2, t2);
						uV[array[j][k]] = value4;
					}
				}
			}
		}
	}

	private static int[][] getFillIndices(dfFillDirection fillDirection, int baseIndex)
	{
		int[][] array = ((fillDirection != 0) ? vertFill : horzFill);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				fillIndices[i][j] = baseIndex + array[i][j];
			}
		}
		return fillIndices;
	}

	private static void rebuildVertices(dfRenderData renderData, RenderOptions options)
	{
		float x = 0f;
		float y = 0f;
		float num = Mathf.Ceil(options.size.x);
		float num2 = Mathf.Ceil(0f - options.size.y);
		dfAtlas.ItemInfo spriteInfo = options.spriteInfo;
		float num3 = spriteInfo.border.left;
		float num4 = spriteInfo.border.top;
		float num5 = spriteInfo.border.right;
		float num6 = spriteInfo.border.bottom;
		if (options.flip.IsSet(dfSpriteFlip.FlipHorizontal))
		{
			float num7 = num5;
			num5 = num3;
			num3 = num7;
		}
		if (options.flip.IsSet(dfSpriteFlip.FlipVertical))
		{
			float num8 = num6;
			num6 = num4;
			num4 = num8;
		}
		verts[0] = new Vector3(x, y, 0f) + options.offset;
		verts[1] = verts[0] + new Vector3(num3, 0f, 0f);
		verts[2] = verts[0] + new Vector3(num3, 0f - num4, 0f);
		verts[3] = verts[0] + new Vector3(0f, 0f - num4, 0f);
		verts[4] = new Vector3(num - num5, y, 0f) + options.offset;
		verts[5] = verts[4] + new Vector3(num5, 0f, 0f);
		verts[6] = verts[4] + new Vector3(num5, 0f - num4, 0f);
		verts[7] = verts[4] + new Vector3(0f, 0f - num4, 0f);
		verts[8] = new Vector3(x, num2 + num6, 0f) + options.offset;
		verts[9] = verts[8] + new Vector3(num3, 0f, 0f);
		verts[10] = verts[8] + new Vector3(num3, 0f - num6, 0f);
		verts[11] = verts[8] + new Vector3(0f, 0f - num6, 0f);
		verts[12] = new Vector3(num - num5, num2 + num6, 0f) + options.offset;
		verts[13] = verts[12] + new Vector3(num5, 0f, 0f);
		verts[14] = verts[12] + new Vector3(num5, 0f - num6, 0f);
		verts[15] = verts[12] + new Vector3(0f, 0f - num6, 0f);
		for (int i = 0; i < verts.Length; i++)
		{
			renderData.Vertices.Add((verts[i] * options.pixelsToUnits).Quantize(options.pixelsToUnits));
		}
	}

	private static void rebuildUV(dfRenderData renderData, RenderOptions options)
	{
		dfAtlas dfAtlas2 = options.atlas;
		Vector2 vector = new Vector2(dfAtlas2.Texture.width, dfAtlas2.Texture.height);
		dfAtlas.ItemInfo spriteInfo = options.spriteInfo;
		float num = (float)spriteInfo.border.top / vector.y;
		float num2 = (float)spriteInfo.border.bottom / vector.y;
		float num3 = (float)spriteInfo.border.left / vector.x;
		float num4 = (float)spriteInfo.border.right / vector.x;
		Rect region = spriteInfo.region;
		uv[0] = new Vector2(region.x, region.yMax);
		uv[1] = new Vector2(region.x + num3, region.yMax);
		uv[2] = new Vector2(region.x + num3, region.yMax - num);
		uv[3] = new Vector2(region.x, region.yMax - num);
		uv[4] = new Vector2(region.xMax - num4, region.yMax);
		uv[5] = new Vector2(region.xMax, region.yMax);
		uv[6] = new Vector2(region.xMax, region.yMax - num);
		uv[7] = new Vector2(region.xMax - num4, region.yMax - num);
		uv[8] = new Vector2(region.x, region.y + num2);
		uv[9] = new Vector2(region.x + num3, region.y + num2);
		uv[10] = new Vector2(region.x + num3, region.y);
		uv[11] = new Vector2(region.x, region.y);
		uv[12] = new Vector2(region.xMax - num4, region.y + num2);
		uv[13] = new Vector2(region.xMax, region.y + num2);
		uv[14] = new Vector2(region.xMax, region.y);
		uv[15] = new Vector2(region.xMax - num4, region.y);
		if (options.flip != 0)
		{
			for (int i = 0; i < uv.Length; i += 4)
			{
				Vector2 zero = Vector2.zero;
				if (options.flip.IsSet(dfSpriteFlip.FlipHorizontal))
				{
					zero = uv[i];
					uv[i] = uv[i + 1];
					uv[i + 1] = zero;
					zero = uv[i + 2];
					uv[i + 2] = uv[i + 3];
					uv[i + 3] = zero;
				}
				if (options.flip.IsSet(dfSpriteFlip.FlipVertical))
				{
					zero = uv[i];
					uv[i] = uv[i + 3];
					uv[i + 3] = zero;
					zero = uv[i + 1];
					uv[i + 1] = uv[i + 2];
					uv[i + 2] = zero;
				}
			}
			if (options.flip.IsSet(dfSpriteFlip.FlipHorizontal))
			{
				Vector2[] array = new Vector2[uv.Length];
				Array.Copy(uv, array, uv.Length);
				Array.Copy(uv, 0, uv, 4, 4);
				Array.Copy(array, 4, uv, 0, 4);
				Array.Copy(uv, 8, uv, 12, 4);
				Array.Copy(array, 12, uv, 8, 4);
			}
			if (options.flip.IsSet(dfSpriteFlip.FlipVertical))
			{
				Vector2[] array2 = new Vector2[uv.Length];
				Array.Copy(uv, array2, uv.Length);
				Array.Copy(uv, 0, uv, 8, 4);
				Array.Copy(array2, 8, uv, 0, 4);
				Array.Copy(uv, 4, uv, 12, 4);
				Array.Copy(array2, 12, uv, 4, 4);
			}
		}
		for (int j = 0; j < uv.Length; j++)
		{
			renderData.UV.Add(uv[j]);
		}
	}

	private static void rebuildColors(dfRenderData renderData, RenderOptions options)
	{
		for (int i = 0; i < 16; i++)
		{
			renderData.Colors.Add(options.color);
		}
	}
}
