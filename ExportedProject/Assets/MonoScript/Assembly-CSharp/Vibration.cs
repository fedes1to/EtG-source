using UnityEngine;

public static class Vibration
{
	public enum Time
	{
		Instant = 5,
		Quick = 10,
		Normal = 20,
		Slow = 30
	}

	public enum Strength
	{
		UltraLight = 5,
		Light = 10,
		Medium = 20,
		Hard = 30
	}

	public static float ConvertFromShakeMagnitude(float magnitude)
	{
		if (magnitude < 0.01f)
		{
			return 0f;
		}
		return 0.4f + Mathf.InverseLerp(0f, 1f, magnitude) * 0.6f;
	}

	public static float ConvertTime(Time time)
	{
		switch (time)
		{
		case Time.Instant:
			return 0f;
		case Time.Quick:
			return 0.15f;
		case Time.Normal:
			return 0.25f;
		case Time.Slow:
			return 0.5f;
		default:
			return 0f;
		}
	}

	public static float ConvertStrength(Strength strength)
	{
		switch (strength)
		{
		case Strength.UltraLight:
			return 0.2f;
		case Strength.Light:
			return 0.4f;
		case Strength.Medium:
			return 0.7f;
		case Strength.Hard:
			return 1f;
		default:
			return 0.5f;
		}
	}
}
