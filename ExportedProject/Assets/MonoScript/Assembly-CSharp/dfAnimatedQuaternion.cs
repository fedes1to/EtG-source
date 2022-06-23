using UnityEngine;

public class dfAnimatedQuaternion : dfAnimatedValue<Quaternion>
{
	public dfAnimatedQuaternion(Quaternion StartValue, Quaternion EndValue, float Time)
		: base(StartValue, EndValue, Time)
	{
	}

	protected override Quaternion Lerp(Quaternion startValue, Quaternion endValue, float time)
	{
		return Quaternion.Lerp(startValue, endValue, time);
	}

	public static implicit operator dfAnimatedQuaternion(Quaternion value)
	{
		return new dfAnimatedQuaternion(value, value, 0f);
	}
}
