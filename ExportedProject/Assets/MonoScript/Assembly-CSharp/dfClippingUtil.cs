using System;
using System.Collections.Generic;
using UnityEngine;

internal class dfClippingUtil
{
	protected struct ClipTriangle
	{
		public Vector3[] corner;

		public Vector2[] uv;

		public Color32[] color;

		public void CopyTo(ref ClipTriangle target)
		{
			Array.Copy(corner, 0, target.corner, 0, 3);
			Array.Copy(uv, 0, target.uv, 0, 3);
			Array.Copy(color, 0, target.color, 0, 3);
		}

		public void CopyTo(dfRenderData buffer)
		{
			int count = buffer.Vertices.Count;
			buffer.Vertices.AddRange(corner);
			buffer.UV.AddRange(uv);
			buffer.Colors.AddRange(color);
			buffer.Triangles.Add(count, count + 1, count + 2);
		}
	}

	private static int[] inside;

	private static ClipTriangle[] clipSource;

	private static ClipTriangle[] clipDest;

	static dfClippingUtil()
	{
		inside = new int[3];
		clipSource = initClipBuffer(1024);
		clipDest = initClipBuffer(1024);
	}

	public static void Clip(IList<Plane> planes, dfRenderData source, dfRenderData dest)
	{
		dest.EnsureCapacity(dest.Vertices.Count + source.Vertices.Count);
		int count = source.Triangles.Count;
		Vector3[] items = source.Vertices.Items;
		int[] items2 = source.Triangles.Items;
		Vector2[] items3 = source.UV.Items;
		Color32[] items4 = source.Colors.Items;
		Matrix4x4 transform = source.Transform;
		int count2 = planes.Count;
		for (int i = 0; i < count; i += 3)
		{
			for (int j = 0; j < 3; j++)
			{
				int num = items2[i + j];
				clipSource[0].corner[j] = transform.MultiplyPoint(items[num]);
				clipSource[0].uv[j] = items3[num];
				clipSource[0].color[j] = items4[num];
			}
			int num2 = 1;
			for (int k = 0; k < count2; k++)
			{
				Plane plane = planes[k];
				num2 = clipToPlane(ref plane, clipSource, clipDest, num2);
				ClipTriangle[] array = clipSource;
				clipSource = clipDest;
				clipDest = array;
			}
			for (int l = 0; l < num2; l++)
			{
				clipSource[l].CopyTo(dest);
			}
		}
	}

	private static int clipToPlane(ref Plane plane, ClipTriangle[] source, ClipTriangle[] dest, int count)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			num += clipToPlane(ref plane, ref source[i], dest, num);
		}
		return num;
	}

	private static int inverseClipToPlane(ref Plane plane, ref ClipTriangle triangle, ClipTriangle[] dest, int destIndex)
	{
		Vector3[] corner = triangle.corner;
		int num = 0;
		int num2 = 0;
		Vector3 lhs = plane.normal * -1f;
		float distance = plane.distance;
		for (int i = 0; i < 3; i++)
		{
			if (Vector3.Dot(lhs, corner[i]) + distance > 0f)
			{
				inside[num++] = i;
			}
			else
			{
				num2 = i;
			}
		}
		switch (num)
		{
		case 3:
		{
			ClipTriangle clipTriangle3 = dest[destIndex];
			Array.Copy(triangle.corner, 0, clipTriangle3.corner, 0, 3);
			Array.Copy(triangle.uv, 0, clipTriangle3.uv, 0, 3);
			Array.Copy(triangle.color, 0, clipTriangle3.color, 0, 3);
			return 1;
		}
		case 0:
			return 0;
		case 1:
		{
			int num6 = inside[0];
			int num7 = (num6 + 1) % 3;
			int num8 = (num6 + 2) % 3;
			Vector3 vector9 = corner[num6];
			Vector3 vector10 = corner[num7];
			Vector3 vector11 = corner[num8];
			Vector2 vector12 = triangle.uv[num6];
			Vector2 b = triangle.uv[num7];
			Vector2 b2 = triangle.uv[num8];
			Color32 color5 = triangle.color[num6];
			Color32 b3 = triangle.color[num7];
			Color32 b4 = triangle.color[num8];
			float enter2 = 0f;
			Vector3 vector13 = vector10 - vector9;
			Ray ray2 = new Ray(vector9, vector13.normalized);
			plane.Raycast(ray2, out enter2);
			float t2 = enter2 / vector13.magnitude;
			Vector3 point3 = ray2.GetPoint(enter2);
			Vector2 vector14 = Vector2.Lerp(vector12, b, t2);
			Color32 color6 = Color32.Lerp(color5, b3, t2);
			vector13 = vector11 - vector9;
			ray2 = new Ray(vector9, vector13.normalized);
			plane.Raycast(ray2, out enter2);
			t2 = enter2 / vector13.magnitude;
			Vector3 point4 = ray2.GetPoint(enter2);
			Vector2 vector15 = Vector2.Lerp(vector12, b2, t2);
			Color32 color7 = Color32.Lerp(color5, b4, t2);
			ClipTriangle clipTriangle2 = dest[destIndex];
			clipTriangle2.corner[0] = vector9;
			clipTriangle2.corner[1] = point3;
			clipTriangle2.corner[2] = point4;
			clipTriangle2.uv[0] = vector12;
			clipTriangle2.uv[1] = vector14;
			clipTriangle2.uv[2] = vector15;
			clipTriangle2.color[0] = color5;
			clipTriangle2.color[1] = color6;
			clipTriangle2.color[2] = color7;
			return 1;
		}
		default:
		{
			int num3 = num2;
			int num4 = (num3 + 1) % 3;
			int num5 = (num3 + 2) % 3;
			Vector3 vector = corner[num3];
			Vector3 vector2 = corner[num4];
			Vector3 vector3 = corner[num5];
			Vector2 a = triangle.uv[num3];
			Vector2 vector4 = triangle.uv[num4];
			Vector2 vector5 = triangle.uv[num5];
			Color32 a2 = triangle.color[num3];
			Color32 color = triangle.color[num4];
			Color32 color2 = triangle.color[num5];
			Vector3 vector6 = vector2 - vector;
			Ray ray = new Ray(vector, vector6.normalized);
			float enter = 0f;
			plane.Raycast(ray, out enter);
			float t = enter / vector6.magnitude;
			Vector3 point = ray.GetPoint(enter);
			Vector2 vector7 = Vector2.Lerp(a, vector4, t);
			Color32 color3 = Color32.Lerp(a2, color, t);
			vector6 = vector3 - vector;
			ray = new Ray(vector, vector6.normalized);
			plane.Raycast(ray, out enter);
			t = enter / vector6.magnitude;
			Vector3 point2 = ray.GetPoint(enter);
			Vector2 vector8 = Vector2.Lerp(a, vector5, t);
			Color32 color4 = Color32.Lerp(a2, color2, t);
			ClipTriangle clipTriangle = dest[destIndex];
			clipTriangle.corner[0] = point;
			clipTriangle.corner[1] = vector2;
			clipTriangle.corner[2] = point2;
			clipTriangle.uv[0] = vector7;
			clipTriangle.uv[1] = vector4;
			clipTriangle.uv[2] = vector8;
			clipTriangle.color[0] = color3;
			clipTriangle.color[1] = color;
			clipTriangle.color[2] = color4;
			clipTriangle = dest[++destIndex];
			clipTriangle.corner[0] = point2;
			clipTriangle.corner[1] = vector2;
			clipTriangle.corner[2] = vector3;
			clipTriangle.uv[0] = vector8;
			clipTriangle.uv[1] = vector4;
			clipTriangle.uv[2] = vector5;
			clipTriangle.color[0] = color4;
			clipTriangle.color[1] = color;
			clipTriangle.color[2] = color2;
			return 2;
		}
		}
	}

	private static int clipToPlane(ref Plane plane, ref ClipTriangle triangle, ClipTriangle[] dest, int destIndex)
	{
		Vector3[] corner = triangle.corner;
		int num = 0;
		int num2 = 0;
		Vector3 normal = plane.normal;
		float distance = plane.distance;
		for (int i = 0; i < 3; i++)
		{
			if (Vector3.Dot(normal, corner[i]) + distance > 0f)
			{
				inside[num++] = i;
			}
			else
			{
				num2 = i;
			}
		}
		switch (num)
		{
		case 3:
		{
			ClipTriangle clipTriangle3 = dest[destIndex];
			Array.Copy(triangle.corner, 0, clipTriangle3.corner, 0, 3);
			Array.Copy(triangle.uv, 0, clipTriangle3.uv, 0, 3);
			Array.Copy(triangle.color, 0, clipTriangle3.color, 0, 3);
			return 1;
		}
		case 0:
			return 0;
		case 1:
		{
			int num6 = inside[0];
			int num7 = (num6 + 1) % 3;
			int num8 = (num6 + 2) % 3;
			Vector3 vector9 = corner[num6];
			Vector3 vector10 = corner[num7];
			Vector3 vector11 = corner[num8];
			Vector2 vector12 = triangle.uv[num6];
			Vector2 b = triangle.uv[num7];
			Vector2 b2 = triangle.uv[num8];
			Color32 color5 = triangle.color[num6];
			Color32 b3 = triangle.color[num7];
			Color32 b4 = triangle.color[num8];
			float enter2 = 0f;
			Vector3 vector13 = vector10 - vector9;
			Ray ray2 = new Ray(vector9, vector13.normalized);
			plane.Raycast(ray2, out enter2);
			float t2 = enter2 / vector13.magnitude;
			Vector3 point3 = ray2.GetPoint(enter2);
			Vector2 vector14 = Vector2.Lerp(vector12, b, t2);
			Color32 color6 = Color32.Lerp(color5, b3, t2);
			vector13 = vector11 - vector9;
			ray2 = new Ray(vector9, vector13.normalized);
			plane.Raycast(ray2, out enter2);
			t2 = enter2 / vector13.magnitude;
			Vector3 point4 = ray2.GetPoint(enter2);
			Vector2 vector15 = Vector2.Lerp(vector12, b2, t2);
			Color32 color7 = Color32.Lerp(color5, b4, t2);
			ClipTriangle clipTriangle2 = dest[destIndex];
			clipTriangle2.corner[0] = vector9;
			clipTriangle2.corner[1] = point3;
			clipTriangle2.corner[2] = point4;
			clipTriangle2.uv[0] = vector12;
			clipTriangle2.uv[1] = vector14;
			clipTriangle2.uv[2] = vector15;
			clipTriangle2.color[0] = color5;
			clipTriangle2.color[1] = color6;
			clipTriangle2.color[2] = color7;
			return 1;
		}
		default:
		{
			int num3 = num2;
			int num4 = (num3 + 1) % 3;
			int num5 = (num3 + 2) % 3;
			Vector3 vector = corner[num3];
			Vector3 vector2 = corner[num4];
			Vector3 vector3 = corner[num5];
			Vector2 a = triangle.uv[num3];
			Vector2 vector4 = triangle.uv[num4];
			Vector2 vector5 = triangle.uv[num5];
			Color32 a2 = triangle.color[num3];
			Color32 color = triangle.color[num4];
			Color32 color2 = triangle.color[num5];
			Vector3 vector6 = vector2 - vector;
			Ray ray = new Ray(vector, vector6.normalized);
			float enter = 0f;
			plane.Raycast(ray, out enter);
			float t = enter / vector6.magnitude;
			Vector3 point = ray.GetPoint(enter);
			Vector2 vector7 = Vector2.Lerp(a, vector4, t);
			Color32 color3 = Color32.Lerp(a2, color, t);
			vector6 = vector3 - vector;
			ray = new Ray(vector, vector6.normalized);
			plane.Raycast(ray, out enter);
			t = enter / vector6.magnitude;
			Vector3 point2 = ray.GetPoint(enter);
			Vector2 vector8 = Vector2.Lerp(a, vector5, t);
			Color32 color4 = Color32.Lerp(a2, color2, t);
			ClipTriangle clipTriangle = dest[destIndex];
			clipTriangle.corner[0] = point;
			clipTriangle.corner[1] = vector2;
			clipTriangle.corner[2] = point2;
			clipTriangle.uv[0] = vector7;
			clipTriangle.uv[1] = vector4;
			clipTriangle.uv[2] = vector8;
			clipTriangle.color[0] = color3;
			clipTriangle.color[1] = color;
			clipTriangle.color[2] = color4;
			clipTriangle = dest[++destIndex];
			clipTriangle.corner[0] = point2;
			clipTriangle.corner[1] = vector2;
			clipTriangle.corner[2] = vector3;
			clipTriangle.uv[0] = vector8;
			clipTriangle.uv[1] = vector4;
			clipTriangle.uv[2] = vector5;
			clipTriangle.color[0] = color4;
			clipTriangle.color[1] = color;
			clipTriangle.color[2] = color2;
			return 2;
		}
		}
	}

	private static ClipTriangle[] initClipBuffer(int size)
	{
		ClipTriangle[] array = new ClipTriangle[size];
		for (int i = 0; i < size; i++)
		{
			array[i].corner = new Vector3[3];
			array[i].uv = new Vector2[3];
			array[i].color = new Color32[3];
		}
		return array;
	}
}
