using UnityEngine;

public static class tk2dSpriteGeomGen
{
	private static readonly int[] boxIndicesBack = new int[36]
	{
		0, 1, 2, 2, 1, 3, 6, 5, 4, 7,
		5, 6, 3, 7, 6, 2, 3, 6, 4, 5,
		1, 4, 1, 0, 6, 4, 0, 6, 0, 2,
		1, 7, 3, 5, 7, 1
	};

	private static readonly int[] boxIndicesFwd = new int[36]
	{
		2, 1, 0, 3, 1, 2, 4, 5, 6, 6,
		5, 7, 6, 7, 3, 6, 3, 2, 1, 5,
		4, 0, 1, 4, 0, 4, 6, 2, 0, 6,
		3, 7, 1, 1, 7, 5
	};

	private static readonly Vector3[] boxUnitVertices = new Vector3[8]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(1f, 1f, 1f)
	};

	private static Matrix4x4 boxScaleMatrix = Matrix4x4.identity;

	public static void SetSpriteColors(Color32[] dest, int offset, int numVertices, Color c, bool premulAlpha)
	{
		if (premulAlpha)
		{
			c.r *= c.a;
			c.g *= c.a;
			c.b *= c.a;
		}
		Color32 color = c;
		for (int i = 0; i < numVertices; i++)
		{
			dest[offset + i] = color;
		}
	}

	public static Vector2 GetAnchorOffset(tk2dBaseSprite.Anchor anchor, float width, float height)
	{
		Vector2 zero = Vector2.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			zero.x = (int)(width / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			zero.x = (int)width;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			zero.y = (int)(height / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerLeft:
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.LowerRight:
			zero.y = (int)height;
			break;
		}
		return zero;
	}

	public static void GetSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef)
	{
		numVertices = 4;
		numIndices = spriteDef.indices.Length;
	}

	public static void SetSpriteGeom(Vector3[] pos, Vector2[] uv, Vector3[] norm, Vector4[] tang, int offset, tk2dSpriteDefinition spriteDef, Vector3 scale)
	{
		pos[offset] = Vector3.Scale(spriteDef.position0, scale);
		pos[offset + 1] = Vector3.Scale(spriteDef.position1, scale);
		pos[offset + 2] = Vector3.Scale(spriteDef.position2, scale);
		pos[offset + 3] = Vector3.Scale(spriteDef.position3, scale);
		for (int i = 0; i < spriteDef.uvs.Length; i++)
		{
			uv[offset + i] = spriteDef.uvs[i];
		}
		if (norm != null && spriteDef.normals != null)
		{
			for (int j = 0; j < spriteDef.normals.Length; j++)
			{
				norm[offset + j] = spriteDef.normals[j];
			}
		}
		if (tang != null && spriteDef.tangents != null)
		{
			for (int k = 0; k < spriteDef.tangents.Length; k++)
			{
				tang[offset + k] = spriteDef.tangents[k];
			}
		}
	}

	public static void SetSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef)
	{
		for (int i = 0; i < spriteDef.indices.Length; i++)
		{
			indices[offset + i] = vStart + spriteDef.indices[i];
		}
	}

	public static void GetClippedSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef)
	{
		numVertices = 4;
		numIndices = 6;
	}

	public static void SetClippedSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 clipBottomLeft, Vector2 clipTopRight, float colliderOffsetZ, float colliderExtentZ)
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		Vector3 vector = spriteDef.untrimmedBoundsDataCenter - spriteDef.untrimmedBoundsDataExtents * 0.5f;
		Vector3 vector2 = spriteDef.untrimmedBoundsDataCenter + spriteDef.untrimmedBoundsDataExtents * 0.5f;
		float num = Mathf.Lerp(vector.x, vector2.x, clipBottomLeft.x);
		float num2 = Mathf.Lerp(vector.x, vector2.x, clipTopRight.x);
		float num3 = Mathf.Lerp(vector.y, vector2.y, clipBottomLeft.y);
		float num4 = Mathf.Lerp(vector.y, vector2.y, clipTopRight.y);
		Vector3 boundsDataExtents = spriteDef.boundsDataExtents;
		Vector3 vector3 = spriteDef.boundsDataCenter - boundsDataExtents * 0.5f;
		float value = (num - vector3.x) / boundsDataExtents.x;
		float value2 = (num2 - vector3.x) / boundsDataExtents.x;
		float value3 = (num3 - vector3.y) / boundsDataExtents.y;
		float value4 = (num4 - vector3.y) / boundsDataExtents.y;
		Vector2 vector4 = new Vector2(Mathf.Clamp01(value), Mathf.Clamp01(value3));
		Vector2 vector5 = new Vector2(Mathf.Clamp01(value2), Mathf.Clamp01(value4));
		Vector3 position = spriteDef.position0;
		Vector3 position2 = spriteDef.position3;
		Vector3 vector6 = new Vector3(Mathf.Lerp(position.x, position2.x, vector4.x) * scale.x, Mathf.Lerp(position.y, position2.y, vector4.y) * scale.y, position.z * scale.z);
		Vector3 vector7 = new Vector3(Mathf.Lerp(position.x, position2.x, vector5.x) * scale.x, Mathf.Lerp(position.y, position2.y, vector5.y) * scale.y, position.z * scale.z);
		boundsCenter.Set(vector6.x + (vector7.x - vector6.x) * 0.5f, vector6.y + (vector7.y - vector6.y) * 0.5f, colliderOffsetZ);
		boundsExtents.Set((vector7.x - vector6.x) * 0.5f, (vector7.y - vector6.y) * 0.5f, colliderExtentZ);
		pos[offset] = new Vector3(vector6.x, vector6.y, vector6.z);
		pos[offset + 1] = new Vector3(vector7.x, vector6.y, vector6.z);
		pos[offset + 2] = new Vector3(vector6.x, vector7.y, vector6.z);
		pos[offset + 3] = new Vector3(vector7.x, vector7.y, vector6.z);
		if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
		{
			Vector2 vector8 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector4.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector4.x));
			Vector2 vector9 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector5.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector5.x));
			uv[offset] = new Vector2(vector8.x, vector8.y);
			uv[offset + 1] = new Vector2(vector8.x, vector9.y);
			uv[offset + 2] = new Vector2(vector9.x, vector8.y);
			uv[offset + 3] = new Vector2(vector9.x, vector9.y);
		}
		else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
		{
			Vector2 vector10 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector4.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector4.x));
			Vector2 vector11 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector5.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector5.x));
			uv[offset] = new Vector2(vector10.x, vector10.y);
			uv[offset + 2] = new Vector2(vector11.x, vector10.y);
			uv[offset + 1] = new Vector2(vector10.x, vector11.y);
			uv[offset + 3] = new Vector2(vector11.x, vector11.y);
		}
		else
		{
			Vector2 vector12 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector4.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector4.y));
			Vector2 vector13 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector5.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector5.y));
			uv[offset] = new Vector2(vector12.x, vector12.y);
			uv[offset + 1] = new Vector2(vector13.x, vector12.y);
			uv[offset + 2] = new Vector2(vector12.x, vector13.y);
			uv[offset + 3] = new Vector2(vector13.x, vector13.y);
		}
	}

	public static void SetClippedSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef)
	{
		indices[offset] = vStart;
		indices[offset + 1] = vStart + 3;
		indices[offset + 2] = vStart + 1;
		indices[offset + 3] = vStart + 2;
		indices[offset + 4] = vStart + 3;
		indices[offset + 5] = vStart;
	}

	public static void GetSlicedSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, bool borderOnly, bool tileStretchedSprite, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, float borderCornerBottom)
	{
		if (tileStretchedSprite)
		{
			GetSlicedTiledSpriteGeomDesc(out numVertices, out numIndices, spriteDef, borderOnly, dimensions, borderBottomLeft, borderTopRight, borderCornerBottom);
			return;
		}
		numVertices = 16;
		numIndices = ((!borderOnly) ? 54 : 48);
	}

	public static void SetSlicedSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, bool borderOnly, Vector3 scale, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, float borderCornerBottom, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ, Vector2 anchorOffset, bool tileStretchedSprite)
	{
		if (tileStretchedSprite)
		{
			SetSlicedTiledSpriteGeom(pos, uv, offset, out boundsCenter, out boundsExtents, spriteDef, borderOnly, scale, dimensions, borderBottomLeft, borderTopRight, borderCornerBottom, anchor, colliderOffsetZ, colliderExtentZ, anchorOffset);
			return;
		}
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		float x = spriteDef.texelSize.x;
		float y = spriteDef.texelSize.y;
		float num = spriteDef.position1.x - spriteDef.position0.x;
		float num2 = spriteDef.position2.y - spriteDef.position0.y;
		float num3 = borderTopRight.y * num2;
		float y2 = borderBottomLeft.y * num2;
		float num4 = borderTopRight.x * num;
		float x2 = borderBottomLeft.x * num;
		float num5 = dimensions.x * x;
		float num6 = dimensions.y * y;
		float num7 = 0f;
		float num8 = 0f;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			num7 = -(int)(dimensions.x / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			num7 = -(int)dimensions.x;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			num8 = -(int)(dimensions.y / 2f);
			break;
		case tk2dBaseSprite.Anchor.UpperLeft:
		case tk2dBaseSprite.Anchor.UpperCenter:
		case tk2dBaseSprite.Anchor.UpperRight:
			num8 = -(int)dimensions.y;
			break;
		}
		num7 -= anchorOffset.x;
		num8 -= anchorOffset.y;
		num7 *= x;
		num8 *= y;
		boundsCenter.Set(scale.x * (num5 * 0.5f + num7), scale.y * (num6 * 0.5f + num8), colliderOffsetZ);
		boundsExtents.Set(scale.x * (num5 * 0.5f), scale.y * (num6 * 0.5f), colliderExtentZ);
		Vector2[] uvs = spriteDef.uvs;
		Vector2 vector = uvs[1] - uvs[0];
		Vector2 vector2 = uvs[2] - uvs[0];
		Vector3 vector3 = new Vector3(num7, num8, 0f);
		Vector3[] array = new Vector3[4]
		{
			vector3,
			vector3 + new Vector3(0f, y2, 0f),
			vector3 + new Vector3(0f, num6 - num3, 0f),
			vector3 + new Vector3(0f, num6, 0f)
		};
		Vector2[] array2 = new Vector2[4]
		{
			uvs[0],
			uvs[0] + vector2 * borderBottomLeft.y,
			uvs[0] + vector2 * (1f - borderTopRight.y),
			uvs[0] + vector2
		};
		for (int i = 0; i < 4; i++)
		{
			pos[offset + i * 4] = array[i];
			pos[offset + i * 4 + 1] = array[i] + new Vector3(x2, 0f, 0f);
			pos[offset + i * 4 + 2] = array[i] + new Vector3(num5 - num4, 0f, 0f);
			pos[offset + i * 4 + 3] = array[i] + new Vector3(num5, 0f, 0f);
			for (int j = 0; j < 4; j++)
			{
				pos[offset + i * 4 + j] = Vector3.Scale(pos[offset + i * 4 + j], scale);
			}
			uv[offset + i * 4] = array2[i];
			uv[offset + i * 4 + 1] = array2[i] + vector * borderBottomLeft.x;
			uv[offset + i * 4 + 2] = array2[i] + vector * (1f - borderTopRight.x);
			uv[offset + i * 4 + 3] = array2[i] + vector;
		}
	}

	public static void SetSlicedSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, bool borderOnly, bool tileStretchedSprite, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, float borderCornerBottom)
	{
		if (tileStretchedSprite)
		{
			SetSlicedTiledSpriteIndices(indices, offset, vStart, spriteDef, borderOnly, dimensions, borderBottomLeft, borderTopRight, borderCornerBottom);
			return;
		}
		int[] array = new int[54]
		{
			0, 4, 1, 1, 4, 5, 1, 5, 2, 2,
			5, 6, 2, 6, 3, 3, 6, 7, 4, 8,
			5, 5, 8, 9, 6, 10, 7, 7, 10, 11,
			8, 12, 9, 9, 12, 13, 9, 13, 10, 10,
			13, 14, 10, 14, 11, 11, 14, 15, 5, 9,
			6, 6, 9, 10
		};
		int num = array.Length;
		if (borderOnly)
		{
			num -= 6;
		}
		for (int i = 0; i < num; i++)
		{
			indices[offset + i] = vStart + array[i];
		}
	}

	public static void GetSlicedTiledSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, bool borderOnly, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, float borderCornerBottom)
	{
		float x = spriteDef.texelSize.x;
		float y = spriteDef.texelSize.y;
		float num = spriteDef.position1.x - spriteDef.position0.x;
		float num2 = spriteDef.position2.y - spriteDef.position0.y;
		float num3 = borderTopRight.y * num2;
		float num4 = borderBottomLeft.y * num2;
		float num5 = borderTopRight.x * num;
		float num6 = borderBottomLeft.x * num;
		float num7 = borderCornerBottom * num2;
		float num8 = dimensions.x * x;
		float num9 = dimensions.y * y;
		float num10 = num - num5 - num6;
		float num11 = num2 - num3 - num4 - num7;
		float f = (num8 - num5 - num6) / num10;
		float f2 = (num9 - num3 - num4) / num11;
		int num12 = Mathf.CeilToInt(f);
		if (num6 > 0f)
		{
			num12++;
		}
		if (num5 > 0f)
		{
			num12++;
		}
		int num13 = Mathf.CeilToInt(f2);
		if (num3 > 0f)
		{
			num13++;
		}
		if (num4 > 0f)
		{
			num13++;
		}
		int num14 = num12 * num13;
		if (borderOnly)
		{
			num14 -= Mathf.CeilToInt(f) * Mathf.CeilToInt(f2);
		}
		if (borderCornerBottom > 0f)
		{
			num14 += num12;
		}
		numVertices = num14 * 4;
		numIndices = num14 * 6;
	}

	public static void SetSlicedTiledSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, bool borderOnly, Vector3 scale, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, float borderCornerBottom, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ, Vector2 anchorOffset)
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		float x = spriteDef.texelSize.x;
		float y = spriteDef.texelSize.y;
		float num = spriteDef.position1.x - spriteDef.position0.x;
		float num2 = spriteDef.position2.y - spriteDef.position0.y;
		float num3 = borderTopRight.y * num2;
		float num4 = borderBottomLeft.y * num2;
		float num5 = borderTopRight.x * num;
		float num6 = borderBottomLeft.x * num;
		float num7 = borderCornerBottom * num2;
		float num8 = dimensions.x * x;
		float num9 = dimensions.y * y;
		float num10 = num - num5 - num6;
		float num11 = num2 - num3 - num4 - num7;
		int num12 = Mathf.CeilToInt((num8 - num5 - num6) / num10);
		int num13 = Mathf.CeilToInt((num9 - num3 - num4) / num11);
		float num14 = 0f;
		float num15 = 0f;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			num14 = -(int)(dimensions.x / 2f);
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			num14 = -(int)dimensions.x;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			num15 = -(int)(dimensions.y / 2f);
			break;
		case tk2dBaseSprite.Anchor.UpperLeft:
		case tk2dBaseSprite.Anchor.UpperCenter:
		case tk2dBaseSprite.Anchor.UpperRight:
			num15 = -(int)dimensions.y;
			break;
		}
		num14 -= anchorOffset.x;
		num15 -= anchorOffset.y;
		num14 *= x;
		num15 *= y;
		boundsCenter.Set(scale.x * (num8 * 0.5f + num14), scale.y * (num9 * 0.5f + num15), colliderOffsetZ);
		boundsExtents.Set(scale.x * (num8 * 0.5f), scale.y * (num9 * 0.5f), colliderExtentZ);
		Vector2[] uvs = spriteDef.uvs;
		Vector2 vector = uvs[1] - uvs[0];
		Vector2 vector2 = uvs[2] - uvs[0];
		Vector3 vector3 = new Vector3(num14, num15, 0f);
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector2 zero3 = Vector2.zero;
		Vector2 zero4 = Vector2.zero;
		for (int i = 0; i < num12 + 2; i++)
		{
			if (i == 0)
			{
				if (num6 == 0f)
				{
					continue;
				}
				zero.x = 0f;
				zero2.x = num6;
				zero3.x = uvs[0].x;
				zero4.x = uvs[0].x + vector.x * borderBottomLeft.x;
			}
			else if (i == num12 + 1)
			{
				if (num5 == 0f)
				{
					continue;
				}
				zero.x = num8 - num5;
				zero2.x = num8;
				zero3.x = uvs[0].x + vector.x * (1f - borderTopRight.x);
				zero4.x = uvs[0].x + vector.x;
			}
			else
			{
				zero.x = num6 + (float)(i - 1) * num10;
				zero2.x = Mathf.Min(num6 + (float)i * num10, num8 - num5);
				zero3.x = uvs[0].x + vector.x * borderBottomLeft.x;
				zero4.x = uvs[0].x + vector.x * (1f - borderTopRight.x);
				if (i == num12)
				{
					zero4.x = Mathf.Lerp(zero3.x, zero4.x, (zero2.x - zero.x) / num10);
				}
			}
			if (borderCornerBottom > 0f)
			{
				zero.y = 0f;
				zero2.y = num7;
				zero3.y = uvs[0].y;
				zero4.y = (uvs[0] + vector2 * borderCornerBottom).y;
				pos[offset] = Vector3.Scale(vector3 + new Vector3(zero.x, 0f - num7, 2f * num7), scale);
				pos[offset + 1] = Vector3.Scale(vector3 + new Vector3(zero2.x, 0f - num7, 2f * num7), scale);
				pos[offset + 2] = Vector3.Scale(vector3 + new Vector3(zero.x, 0f, 0f), scale);
				pos[offset + 3] = Vector3.Scale(vector3 + new Vector3(zero2.x, 0f, 0f), scale);
				uv[offset] = new Vector2(zero3.x, zero3.y);
				uv[offset + 1] = new Vector2(zero4.x, zero3.y);
				uv[offset + 2] = new Vector2(zero3.x, zero4.y);
				uv[offset + 3] = new Vector2(zero4.x, zero4.y);
				offset += 4;
			}
			for (int j = 0; j < num13 + 2; j++)
			{
				if (j == 0)
				{
					if (num4 == 0f)
					{
						continue;
					}
					zero.y = 0f;
					zero2.y = num4;
					zero3.y = (uvs[0] + vector2 * borderCornerBottom).y;
					zero4.y = (uvs[0] + vector2 * (borderBottomLeft.y + borderCornerBottom)).y;
				}
				else if (j == num13 + 1)
				{
					if (num3 == 0f)
					{
						continue;
					}
					zero.y = num9 - num3;
					zero2.y = num9;
					zero3.y = uvs[0].y + vector2.y * (1f - borderTopRight.y);
					zero4.y = uvs[0].y + vector2.y;
				}
				else
				{
					if (borderOnly && i != 0 && i != num12 + 1)
					{
						continue;
					}
					zero.y = num4 + (float)(j - 1) * num11;
					zero2.y = Mathf.Min(num4 + (float)j * num11, num9 - num3);
					zero3.y = uvs[0].y + vector2.y * (borderBottomLeft.y + borderCornerBottom);
					zero4.y = uvs[0].y + vector2.y * (1f - borderTopRight.y);
					if (j == num13)
					{
						zero4.y = Mathf.Lerp(zero3.y, zero4.y, (zero2.y - zero.y) / num11);
					}
				}
				pos[offset] = Vector3.Scale(vector3 + new Vector3(zero.x, zero.y), scale);
				pos[offset + 1] = Vector3.Scale(vector3 + new Vector3(zero2.x, zero.y), scale);
				pos[offset + 2] = Vector3.Scale(vector3 + new Vector3(zero.x, zero2.y), scale);
				pos[offset + 3] = Vector3.Scale(vector3 + new Vector3(zero2.x, zero2.y), scale);
				uv[offset] = new Vector2(zero3.x, zero3.y);
				uv[offset + 1] = new Vector2(zero4.x, zero3.y);
				uv[offset + 2] = new Vector2(zero3.x, zero4.y);
				uv[offset + 3] = new Vector2(zero4.x, zero4.y);
				offset += 4;
			}
		}
	}

	public static void SetSlicedTiledSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, bool borderOnly, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, float borderCornerBottom)
	{
		int numVertices;
		int numIndices;
		GetSlicedTiledSpriteGeomDesc(out numVertices, out numIndices, spriteDef, borderOnly, dimensions, borderBottomLeft, borderTopRight, borderCornerBottom);
		int num = 0;
		for (int i = 0; i < numIndices; i += 6)
		{
			indices[offset + i] = vStart + spriteDef.indices[0] + num;
			indices[offset + i + 1] = vStart + spriteDef.indices[1] + num;
			indices[offset + i + 2] = vStart + spriteDef.indices[2] + num;
			indices[offset + i + 3] = vStart + spriteDef.indices[3] + num;
			indices[offset + i + 4] = vStart + spriteDef.indices[4] + num;
			indices[offset + i + 5] = vStart + spriteDef.indices[5] + num;
			num += 4;
		}
	}

	public static void GetTiledSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		int num = (int)Mathf.Ceil(dimensions.x * spriteDef.texelSize.x / spriteDef.untrimmedBoundsDataExtents.x);
		int num2 = (int)Mathf.Ceil(dimensions.y * spriteDef.texelSize.y / spriteDef.untrimmedBoundsDataExtents.y);
		numVertices = num * num2 * 4;
		numIndices = num * num2 * 6;
	}

	public static void SetTiledSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		int num = (int)Mathf.Ceil(dimensions.x * spriteDef.texelSize.x / spriteDef.untrimmedBoundsDataExtents.x);
		int num2 = (int)Mathf.Ceil(dimensions.y * spriteDef.texelSize.y / spriteDef.untrimmedBoundsDataExtents.y);
		Vector2 vector = new Vector2(dimensions.x * spriteDef.texelSize.x * scale.x, dimensions.y * spriteDef.texelSize.y * scale.y);
		Vector2 vector2 = Vector2.Scale(spriteDef.texelSize, scale) * 0.1f;
		Vector3 zero = Vector3.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerCenter:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.UpperCenter:
			zero.x = 0f - vector.x / 2f;
			break;
		case tk2dBaseSprite.Anchor.LowerRight:
		case tk2dBaseSprite.Anchor.MiddleRight:
		case tk2dBaseSprite.Anchor.UpperRight:
			zero.x = 0f - vector.x;
			break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.MiddleLeft:
		case tk2dBaseSprite.Anchor.MiddleCenter:
		case tk2dBaseSprite.Anchor.MiddleRight:
			zero.y = 0f - vector.y / 2f;
			break;
		case tk2dBaseSprite.Anchor.UpperLeft:
		case tk2dBaseSprite.Anchor.UpperCenter:
		case tk2dBaseSprite.Anchor.UpperRight:
			zero.y = 0f - vector.y;
			break;
		}
		Vector3 vector3 = zero;
		zero -= Vector3.Scale(spriteDef.position0, scale);
		boundsCenter.Set(vector.x * 0.5f + vector3.x, vector.y * 0.5f + vector3.y, colliderOffsetZ);
		boundsExtents.Set(vector.x * 0.5f, vector.y * 0.5f, colliderExtentZ);
		int num3 = 0;
		Vector3 vector4 = Vector3.Scale(spriteDef.untrimmedBoundsDataExtents, scale);
		Vector3 zero2 = Vector3.zero;
		Vector3 vector5 = zero2;
		for (int i = 0; i < num2; i++)
		{
			vector5.x = zero2.x;
			for (int j = 0; j < num; j++)
			{
				float num4 = 1f;
				float num5 = 1f;
				if (Mathf.Abs(vector5.x + vector4.x) > Mathf.Abs(vector.x) + vector2.x)
				{
					num4 = vector.x % vector4.x / vector4.x;
				}
				if (Mathf.Abs(vector5.y + vector4.y) > Mathf.Abs(vector.y) + vector2.y)
				{
					num5 = vector.y % vector4.y / vector4.y;
				}
				Vector3 vector6 = vector5 + zero;
				if (num4 != 1f || num5 != 1f)
				{
					Vector2 zero3 = Vector2.zero;
					Vector2 vector7 = new Vector2(num4, num5);
					Vector3 vector8 = new Vector3(Mathf.Lerp(spriteDef.position0.x, spriteDef.position3.x, zero3.x) * scale.x, Mathf.Lerp(spriteDef.position0.y, spriteDef.position3.y, zero3.y) * scale.y, spriteDef.position0.z * scale.z);
					Vector3 vector9 = new Vector3(Mathf.Lerp(spriteDef.position0.x, spriteDef.position3.x, vector7.x) * scale.x, Mathf.Lerp(spriteDef.position0.y, spriteDef.position3.y, vector7.y) * scale.y, spriteDef.position0.z * scale.z);
					pos[offset + num3] = vector6 + new Vector3(vector8.x, vector8.y, vector8.z);
					pos[offset + num3 + 1] = vector6 + new Vector3(vector9.x, vector8.y, vector8.z);
					pos[offset + num3 + 2] = vector6 + new Vector3(vector8.x, vector9.y, vector8.z);
					pos[offset + num3 + 3] = vector6 + new Vector3(vector9.x, vector9.y, vector8.z);
					if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
					{
						Vector2 vector10 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, zero3.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, zero3.x));
						Vector2 vector11 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector7.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector7.x));
						uv[offset + num3] = new Vector2(vector10.x, vector10.y);
						uv[offset + num3 + 1] = new Vector2(vector10.x, vector11.y);
						uv[offset + num3 + 2] = new Vector2(vector11.x, vector10.y);
						uv[offset + num3 + 3] = new Vector2(vector11.x, vector11.y);
					}
					else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
					{
						Vector2 vector12 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, zero3.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, zero3.x));
						Vector2 vector13 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector7.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector7.x));
						uv[offset + num3] = new Vector2(vector12.x, vector12.y);
						uv[offset + num3 + 2] = new Vector2(vector13.x, vector12.y);
						uv[offset + num3 + 1] = new Vector2(vector12.x, vector13.y);
						uv[offset + num3 + 3] = new Vector2(vector13.x, vector13.y);
					}
					else
					{
						Vector2 vector14 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, zero3.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, zero3.y));
						Vector2 vector15 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector7.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector7.y));
						uv[offset + num3] = new Vector2(vector14.x, vector14.y);
						uv[offset + num3 + 1] = new Vector2(vector15.x, vector14.y);
						uv[offset + num3 + 2] = new Vector2(vector14.x, vector15.y);
						uv[offset + num3 + 3] = new Vector2(vector15.x, vector15.y);
					}
				}
				else
				{
					pos[offset + num3] = vector6 + Vector3.Scale(spriteDef.position0, scale);
					pos[offset + num3 + 1] = vector6 + Vector3.Scale(spriteDef.position1, scale);
					pos[offset + num3 + 2] = vector6 + Vector3.Scale(spriteDef.position2, scale);
					pos[offset + num3 + 3] = vector6 + Vector3.Scale(spriteDef.position3, scale);
					uv[offset + num3] = spriteDef.uvs[0];
					uv[offset + num3 + 1] = spriteDef.uvs[1];
					uv[offset + num3 + 2] = spriteDef.uvs[2];
					uv[offset + num3 + 3] = spriteDef.uvs[3];
				}
				num3 += 4;
				vector5.x += vector4.x;
			}
			vector5.y += vector4.y;
		}
	}

	public static void SetTiledSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, Vector2 dimensions, tk2dTiledSprite.OverrideGetTiledSpriteGeomDescDelegate overrideGetTiledSpriteGeomDesc = null)
	{
		int numVertices;
		int numIndices;
		if (overrideGetTiledSpriteGeomDesc != null)
		{
			overrideGetTiledSpriteGeomDesc(out numVertices, out numIndices, spriteDef, dimensions);
		}
		else
		{
			GetTiledSpriteGeomDesc(out numVertices, out numIndices, spriteDef, dimensions);
		}
		int num = 0;
		for (int i = 0; i < numIndices; i += 6)
		{
			indices[offset + i] = vStart + spriteDef.indices[0] + num;
			indices[offset + i + 1] = vStart + spriteDef.indices[1] + num;
			indices[offset + i + 2] = vStart + spriteDef.indices[2] + num;
			indices[offset + i + 3] = vStart + spriteDef.indices[3] + num;
			indices[offset + i + 4] = vStart + spriteDef.indices[4] + num;
			indices[offset + i + 5] = vStart + spriteDef.indices[5] + num;
			num += 4;
		}
		for (int j = offset + numIndices; j < indices.Length; j++)
		{
			indices[j] = 0;
		}
	}

	public static void SetBoxMeshData(Vector3[] pos, int[] indices, int posOffset, int indicesOffset, int vStart, Vector3 origin, Vector3 extents, Matrix4x4 mat, Vector3 baseScale)
	{
		boxScaleMatrix.m03 = origin.x * baseScale.x;
		boxScaleMatrix.m13 = origin.y * baseScale.y;
		boxScaleMatrix.m23 = origin.z * baseScale.z;
		boxScaleMatrix.m00 = extents.x * baseScale.x;
		boxScaleMatrix.m11 = extents.y * baseScale.y;
		boxScaleMatrix.m22 = extents.z * baseScale.z;
		Matrix4x4 matrix4x = mat * boxScaleMatrix;
		for (int i = 0; i < 8; i++)
		{
			pos[posOffset + i] = matrix4x.MultiplyPoint(boxUnitVertices[i]);
		}
		float num = mat.m00 * mat.m11 * mat.m22 * baseScale.x * baseScale.y * baseScale.z;
		int[] array = ((!(num >= 0f)) ? boxIndicesBack : boxIndicesFwd);
		for (int j = 0; j < array.Length; j++)
		{
			indices[indicesOffset + j] = vStart + array[j];
		}
	}

	public static void SetSpriteDefinitionMeshData(Vector3[] pos, int[] indices, int posOffset, int indicesOffset, int vStart, tk2dSpriteDefinition spriteDef, Matrix4x4 mat, Vector3 baseScale)
	{
		for (int i = 0; i < spriteDef.colliderVertices.Length; i++)
		{
			Vector3 point = Vector3.Scale(spriteDef.colliderVertices[i], baseScale);
			point = mat.MultiplyPoint(point);
			pos[posOffset + i] = point;
		}
		int[] indices2 = spriteDef.indices;
		for (int j = 0; j < indices2.Length; j++)
		{
			indices[indicesOffset + j] = vStart + indices2[j];
		}
	}

	public static void SetSpriteVertexNormals(Vector3[] pos, Vector3 pMin, Vector3 pMax, Vector3[] spriteDefNormals, Vector4[] spriteDefTangents, Vector3[] normals, Vector4[] tangents)
	{
		Vector3 vector = pMax - pMin;
		int num = pos.Length;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector2 = pos[i];
			float num2 = (vector2.x - pMin.x) / vector.x;
			float num3 = (vector2.y - pMin.y) / vector.y;
			float num4 = (1f - num2) * (1f - num3);
			float num5 = num2 * (1f - num3);
			float num6 = (1f - num2) * num3;
			float num7 = num2 * num3;
			if (spriteDefNormals != null && spriteDefNormals.Length == 4 && i < normals.Length)
			{
				normals[i] = spriteDefNormals[0] * num4 + spriteDefNormals[1] * num5 + spriteDefNormals[2] * num6 + spriteDefNormals[3] * num7;
			}
			if (spriteDefTangents != null && spriteDefTangents.Length == 4 && i < tangents.Length)
			{
				tangents[i] = spriteDefTangents[0] * num4 + spriteDefTangents[1] * num5 + spriteDefTangents[2] * num6 + spriteDefTangents[3] * num7;
			}
		}
	}

	public static void SetSpriteVertexNormalsFast(Vector3[] pos, Vector3[] normals, Vector4[] tangents)
	{
		int num = pos.Length;
		Vector3 back = Vector3.back;
		Vector4 vector = new Vector4(1f, 0f, 0f, 1f);
		for (int i = 0; i < num; i++)
		{
			normals[i] = back;
			tangents[i] = vector;
		}
	}
}
