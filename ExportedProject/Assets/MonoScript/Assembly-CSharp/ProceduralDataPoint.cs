using System;
using UnityEngine;

[Serializable]
public class ProceduralDataPoint
{
	public float minValue;

	public float maxValue;

	public AnimationCurve distribution;

	private const float INTEGRATION_STEP = 0.01f;

	public ProceduralDataPoint(float min, float max)
	{
		minValue = min;
		maxValue = max;
		distribution = new AnimationCurve();
	}

	private float FullIntegortion()
	{
		float num = 0f;
		for (float num2 = 0.01f; num2 <= 1f; num2 += 0.01f)
		{
			num += 0.01f * distribution.Evaluate(num2);
		}
		return num;
	}

	private float PartialIntegortion(float target)
	{
		float num = FullIntegortion();
		float num2 = 0f;
		for (float num3 = 0.01f; num3 <= 1f; num3 += 0.01f)
		{
			num2 += 0.01f * distribution.Evaluate(num3);
			if (num2 / num > target)
			{
				return num3;
			}
		}
		return 1f;
	}

	public float GetSpecificValue(float p)
	{
		return minValue + PartialIntegortion(p) * (maxValue - minValue);
	}

	public int GetSpecificIntValue(float p)
	{
		return Mathf.RoundToInt(GetSpecificValue(p));
	}

	public float GetRandomValue()
	{
		return GetSpecificValue(UnityEngine.Random.value);
	}

	public int GetRandomIntValue()
	{
		return Mathf.RoundToInt(GetRandomValue());
	}
}
