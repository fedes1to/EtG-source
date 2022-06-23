using UnityEngine;

public static class dfNumberExtensions
{
	public static int Quantize(this int value, int stepSize)
	{
		if (stepSize <= 0)
		{
			return value;
		}
		return value / stepSize * stepSize;
	}

	public static float Quantize(this float value, float stepSize)
	{
		if (stepSize <= 0f)
		{
			return value;
		}
		return Mathf.Floor(value / stepSize) * stepSize;
	}

	public static float Quantize(this float value, float stepSize, VectorConversions conversionMethod)
	{
		if (stepSize <= 0f)
		{
			return value;
		}
		switch (conversionMethod)
		{
		case VectorConversions.Floor:
			return Mathf.Floor(value / stepSize) * stepSize;
		case VectorConversions.Ceil:
			return Mathf.Ceil(value / stepSize) * stepSize;
		case VectorConversions.Round:
			return Mathf.Round(value / stepSize) * stepSize;
		default:
			return Mathf.Round(value / stepSize) * stepSize;
		}
	}

	public static int RoundToNearest(this int value, int stepSize)
	{
		if (stepSize <= 0)
		{
			return value;
		}
		int num = value / stepSize * stepSize;
		int num2 = value % stepSize;
		if (num2 >= stepSize / 2)
		{
			return num + stepSize;
		}
		return num;
	}

	public static float RoundToNearest(this float value, float stepSize)
	{
		if (stepSize <= 0f)
		{
			return value;
		}
		float num = Mathf.Floor(value / stepSize) * stepSize;
		float num2 = value - stepSize * Mathf.Floor(value / stepSize);
		if (num2 >= stepSize * 0.5f - float.Epsilon)
		{
			return num + stepSize;
		}
		return num;
	}
}
