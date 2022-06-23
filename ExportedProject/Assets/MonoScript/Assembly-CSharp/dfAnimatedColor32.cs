using UnityEngine;

public class dfAnimatedColor32 : dfAnimatedValue<Color32>
{
	public dfAnimatedColor32(Color32 StartValue, Color32 EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override Color32 Lerp(Color32 startValue, Color32 endValue, float time)
	{
		return Color.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedColor32(Color32 value)
	{
		return new dfAnimatedColor32(value, value, 0f);
	}
}
