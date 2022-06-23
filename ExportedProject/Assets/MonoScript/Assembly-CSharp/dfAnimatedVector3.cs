using UnityEngine;

public class dfAnimatedVector3 : dfAnimatedValue<Vector3>
{
	public dfAnimatedVector3(Vector3 StartValue, Vector3 EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override Vector3 Lerp(Vector3 startValue, Vector3 endValue, float time)
	{
		return Vector3.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedVector3(Vector3 value)
	{
		return new dfAnimatedVector3(value, value, 0f);
	}
}
