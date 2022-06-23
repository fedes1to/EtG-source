using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Vector3")]
public class dfTweenVector3 : dfTweenComponent<Vector3>
{
	public override Vector3 offset(Vector3 lhs, Vector3 rhs)
	{
		return lhs + rhs;
	}

	public override Vector3 evaluate(Vector3 startValue, Vector3 endValue, float time)
	{
		return new Vector3(dfTweenComponent<Vector3>.Lerp(startValue.x, endValue.x, time), dfTweenComponent<Vector3>.Lerp(startValue.y, endValue.y, time), dfTweenComponent<Vector3>.Lerp(startValue.z, endValue.z, time));
	}
}
