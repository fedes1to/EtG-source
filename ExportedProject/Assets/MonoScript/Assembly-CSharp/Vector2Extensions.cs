using System;
using UnityEngine;

public static class Vector2Extensions
{
	public static Vector2 min
	{
		get
		{
			return new Vector2(float.MinValue, float.MinValue);
		}
	}

	public static Vector2 max
	{
		get
		{
			return new Vector2(float.MaxValue, float.MaxValue);
		}
	}

	public static bool Approximately(this Vector2 vector, Vector2 other)
	{
		return Mathf.Approximately(vector.x, other.x) && Mathf.Approximately(vector.y, other.y);
	}

	public static float ComponentProduct(this Vector2 vector)
	{
		return vector.x * vector.y;
	}

	public static Vector2 WithX(this Vector2 vector, float x)
	{
		return new Vector2(x, vector.y);
	}

	public static Vector2 WithY(this Vector2 vector, float y)
	{
		return new Vector2(vector.x, y);
	}

	public static Vector2 Rotate(this Vector2 v, float degrees)
	{
		float num = Mathf.Sin(degrees * ((float)Math.PI / 180f));
		float num2 = Mathf.Cos(degrees * ((float)Math.PI / 180f));
		float x = v.x;
		float y = v.y;
		v.x = num2 * x - num * y;
		v.y = num * x + num2 * y;
		return v;
	}

	public static Vector4 ToVector4(this Vector2 vector)
	{
		return new Vector4(vector.x, vector.y, 0f, 0f);
	}

	public static IntVector2 ToIntVector2(this Vector2 vector, VectorConversions convertMethod = VectorConversions.Round)
	{
		switch (convertMethod)
		{
		case VectorConversions.Ceil:
			return new IntVector2(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y));
		case VectorConversions.Floor:
			return new IntVector2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		default:
			return new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
		}
	}

	public static Vector3 ToVector3XUp(this Vector2 vector, float x = 0f)
	{
		return new Vector3(x, vector.x, vector.y);
	}

	public static Vector3 ToVector3YUp(this Vector2 vector, float y = 0f)
	{
		return new Vector3(vector.x, y, vector.y);
	}

	public static Vector3 ToVector3ZUp(this Vector2 vector, float z = 0f)
	{
		return new Vector3(vector.x, vector.y, z);
	}

	public static Vector3 ToVector3ZisY(this Vector2 vector, float offset = 0f)
	{
		return new Vector3(vector.x, vector.y, vector.y + offset);
	}

	public static bool IsWithin(this Vector2 vector, Vector2 min, Vector2 max)
	{
		return vector.x >= min.x && vector.x <= max.x && vector.y >= min.y && vector.y <= max.y;
	}

	public static Vector2 Clamp(Vector2 vector, Vector2 min, Vector2 max)
	{
		return new Vector2(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y));
	}

	public static float ToAngle(this Vector2 vector)
	{
		return BraveMathCollege.Atan2Degrees(vector);
	}

	public static Vector2 Abs(this Vector2 vector)
	{
		return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}

	public static float Cross(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	public static Vector2 Cross(Vector2 a, float s)
	{
		return new Vector2(s * a.y, (0f - s) * a.x);
	}

	public static Vector2 Cross(float s, Vector2 a)
	{
		return new Vector2((0f - s) * a.y, s * a.x);
	}

	public static bool IsHorizontal(this Vector2 vector)
	{
		return Mathf.Abs(vector.x) > 0f && vector.y == 0f;
	}

	public static Vector2 SmoothStep(Vector2 from, Vector2 to, float t)
	{
		return new Vector2(Mathf.SmoothStep(from.x, to.x, t), Mathf.SmoothStep(from.y, to.y, t));
	}

	public static float SqrDistance(Vector2 a, Vector2 b)
	{
		double num = a.x - b.x;
		double num2 = a.y - b.y;
		return (float)(num * num + num2 * num2);
	}
}
