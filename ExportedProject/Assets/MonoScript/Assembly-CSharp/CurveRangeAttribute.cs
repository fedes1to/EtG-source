using UnityEngine;

public class CurveRangeAttribute : PropertyAttribute
{
	public Color Color;

	public Rect Range;

	public CurveRangeAttribute(float xMin, float yMin, float xMax, float yMax)
	{
		Range = new Rect(xMin, yMin, xMax, yMax);
		Color = Color.green;
	}

	public CurveRangeAttribute(float xMin, float yMin, float xMax, float yMax, Color color)
	{
		Range = new Rect(xMin, yMin, xMax, yMax);
		Color = color;
	}
}
