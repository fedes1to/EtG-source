using UnityEngine;

public class dfAnimatedColor : dfAnimatedValue<Color>
{
	public dfAnimatedColor(Color StartValue, Color EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override Color Lerp(Color startValue, Color endValue, float time)
	{
		return Color.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedColor(Color value)
	{
		return new dfAnimatedColor(value, value, 0f);
	}
}
