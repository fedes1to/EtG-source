using UnityEngine;

public class dfAnimatedInt : dfAnimatedValue<int>
{
	public dfAnimatedInt(int StartValue, int EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override int Lerp(int startValue, int endValue, float time)
	{
		return Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, time));
	}

	public static implicit operator dfAnimatedInt(int value)
	{
		return new dfAnimatedInt(value, value, 0f);
	}
}
