using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Float")]
public class dfTweenFloat : dfTweenComponent<float>
{
	public override float offset(float lhs, float rhs)
	{
		return lhs + rhs;
	}

	public override float evaluate(float startValue, float endValue, float time)
	{
		return startValue + (endValue - startValue) * time;
	}
}
