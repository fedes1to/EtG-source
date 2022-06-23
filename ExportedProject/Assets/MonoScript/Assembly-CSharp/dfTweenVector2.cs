using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Vector2")]
public class dfTweenVector2 : dfTweenComponent<Vector2>
{
	public override Vector2 offset(Vector2 lhs, Vector2 rhs)
	{
		return lhs + rhs;
	}

	public override Vector2 evaluate(Vector2 startValue, Vector2 endValue, float time)
	{
		return new Vector2(dfTweenComponent<Vector2>.Lerp(startValue.x, endValue.x, time), dfTweenComponent<Vector2>.Lerp(startValue.y, endValue.y, time));
	}
}
