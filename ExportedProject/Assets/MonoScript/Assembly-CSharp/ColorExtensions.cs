using UnityEngine;

public static class ColorExtensions
{
	public static bool EqualsNonAlloc(this Color32 color, Color32 other)
	{
		return color.r == other.r && color.g == other.g && color.b == other.b && color.a == other.a;
	}

	public static Color SmoothStep(Color start, Color end, float t)
	{
		return new Color(Mathf.SmoothStep(start.r, end.r, t), Mathf.SmoothStep(start.g, end.g, t), Mathf.SmoothStep(start.b, end.b, t), Mathf.SmoothStep(start.a, end.a, t));
	}
}
