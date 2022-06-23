using UnityEngine;

public class dfAnimatedVector2 : dfAnimatedValue<Vector2>
{
	public dfAnimatedVector2(Vector2 StartValue, Vector2 EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override Vector2 Lerp(Vector2 startValue, Vector2 endValue, float time)
	{
		return Vector2.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedVector2(Vector2 value)
	{
		return new dfAnimatedVector2(value, value, 0f);
	}
}
