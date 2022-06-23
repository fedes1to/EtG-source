using UnityEngine;

public class dfAnimatedVector4 : dfAnimatedValue<Vector4>
{
	public dfAnimatedVector4(Vector4 StartValue, Vector4 EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override Vector4 Lerp(Vector4 startValue, Vector4 endValue, float time)
	{
		return Vector4.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedVector4(Vector4 value)
	{
		return new dfAnimatedVector4(value, value, 0f);
	}
}
