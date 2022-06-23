using UnityEngine;

public static class dfVectorExtensions
{
	public static bool IsNaN(this Vector3 vector)
	{
		return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
	}

	public static Vector3 ClampRotation(this Vector3 euler)
	{
		if (euler.x < 0f)
		{
			euler.x += 360f;
		}
		if (euler.x >= 360f)
		{
			euler.x -= 360f;
		}
		if (euler.y < 0f)
		{
			euler.y += 360f;
		}
		if (euler.y >= 360f)
		{
			euler.y -= 360f;
		}
		if (euler.z < 0f)
		{
			euler.z += 360f;
		}
		if (euler.z >= 360f)
		{
			euler.z -= 360f;
		}
		return euler;
	}

	public static Vector2 Scale(this Vector2 vector, float x, float y)
	{
		return new Vector2(vector.x * x, vector.y * y);
	}

	public static Vector3 Scale(this Vector3 vector, float x, float y, float z)
	{
		return new Vector3(vector.x * x, vector.y * y, vector.z * z);
	}

	public static Vector3 FloorToInt(this Vector3 vector)
	{
		return new Vector3(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
	}

	public static Vector3 CeilToInt(this Vector3 vector)
	{
		return new Vector3(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y), Mathf.CeilToInt(vector.z));
	}

	public static Vector2 FloorToInt(this Vector2 vector)
	{
		return new Vector2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
	}

	public static Vector2 CeilToInt(this Vector2 vector)
	{
		return new Vector2(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y));
	}

	public static Vector3 RoundToInt(this Vector3 vector)
	{
		return new Vector3(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	public static Vector2 RoundToInt(this Vector2 vector)
	{
		return new Vector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	public static Vector2 Quantize(this Vector2 vector, float discreteValue)
	{
		vector.x = (float)Mathf.RoundToInt(vector.x / discreteValue) * discreteValue;
		vector.y = (float)Mathf.RoundToInt(vector.y / discreteValue) * discreteValue;
		return vector;
	}

	public static Vector2 Quantize(this Vector2 vector, float discreteValue, VectorConversions conversionMethod)
	{
		switch (conversionMethod)
		{
		case VectorConversions.Ceil:
			vector.x = (float)Mathf.CeilToInt(vector.x / discreteValue) * discreteValue;
			vector.y = (float)Mathf.CeilToInt(vector.y / discreteValue) * discreteValue;
			return vector;
		case VectorConversions.Floor:
			vector.x = (float)Mathf.FloorToInt(vector.x / discreteValue) * discreteValue;
			vector.y = (float)Mathf.FloorToInt(vector.y / discreteValue) * discreteValue;
			return vector;
		default:
			return vector.Quantize(discreteValue);
		}
	}

	public static Vector3 Quantize(this Vector3 vector, float discreteValue, VectorConversions conversionMethod)
	{
		switch (conversionMethod)
		{
		case VectorConversions.Ceil:
			vector.x = (float)Mathf.CeilToInt(vector.x / discreteValue) * discreteValue;
			vector.y = (float)Mathf.CeilToInt(vector.y / discreteValue) * discreteValue;
			vector.z = (float)Mathf.CeilToInt(vector.z / discreteValue) * discreteValue;
			return vector;
		case VectorConversions.Floor:
			vector.x = (float)Mathf.FloorToInt(vector.x / discreteValue) * discreteValue;
			vector.y = (float)Mathf.FloorToInt(vector.y / discreteValue) * discreteValue;
			vector.z = (float)Mathf.FloorToInt(vector.z / discreteValue) * discreteValue;
			return vector;
		default:
			return vector.Quantize(discreteValue);
		}
	}

	public static Vector3 Quantize(this Vector3 vector, float discreteValue)
	{
		vector.x = (float)Mathf.RoundToInt(vector.x / discreteValue) * discreteValue;
		vector.y = (float)Mathf.RoundToInt(vector.y / discreteValue) * discreteValue;
		vector.z = (float)Mathf.RoundToInt(vector.z / discreteValue) * discreteValue;
		return vector;
	}

	public static Vector3 QuantizeFloor(this Vector3 vector, float discreteValue)
	{
		vector.x = (float)Mathf.FloorToInt(vector.x / discreteValue) * discreteValue;
		vector.y = (float)Mathf.FloorToInt(vector.y / discreteValue) * discreteValue;
		vector.z = (float)Mathf.FloorToInt(vector.z / discreteValue) * discreteValue;
		return vector;
	}
}
