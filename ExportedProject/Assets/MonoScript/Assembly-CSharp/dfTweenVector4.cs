using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Vector4")]
public class dfTweenVector4 : dfTweenComponent<Vector4>
{
	public override Vector4 offset(Vector4 lhs, Vector4 rhs)
	{
		return lhs + rhs;
	}

	public override Vector4 evaluate(Vector4 startValue, Vector4 endValue, float time)
	{
		return new Vector4(dfTweenComponent<Vector4>.Lerp(startValue.x, endValue.x, time), dfTweenComponent<Vector4>.Lerp(startValue.y, endValue.y, time), dfTweenComponent<Vector4>.Lerp(startValue.z, endValue.z, time), dfTweenComponent<Vector4>.Lerp(startValue.w, endValue.w, time));
	}
}
