using UnityEngine;

public class dfAnimatedFloat : dfAnimatedValue<float>
{
	public dfAnimatedFloat(float StartValue, float EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override float Lerp(float startValue, float endValue, float time)
	{
		return Mathf.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedFloat(float value)
	{
		return new dfAnimatedFloat(value, value, 0f);
	}
}
