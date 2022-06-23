using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Rotation")]
public class dfTweenRotation : dfTweenComponent<Quaternion>
{
	public override Quaternion offset(Quaternion lhs, Quaternion rhs)
	{
		return lhs * rhs;
	}

	public override Quaternion evaluate(Quaternion startValue, Quaternion endValue, float time)
	{
		Vector3 eulerAngles = startValue.eulerAngles;
		Vector3 eulerAngles2 = endValue.eulerAngles;
		return Quaternion.Euler(LerpEuler(eulerAngles, eulerAngles2, time));
	}

	private static Vector3 LerpEuler(Vector3 startValue, Vector3 endValue, float time)
	{
		return new Vector3(LerpAngle(startValue.x, endValue.x, time), LerpAngle(startValue.y, endValue.y, time), LerpAngle(startValue.z, endValue.z, time));
	}

	private static float LerpAngle(float startValue, float endValue, float time)
	{
		float num = Mathf.Repeat(endValue - startValue, 360f);
		if (num > 180f)
		{
			num -= 360f;
		}
		return startValue + num * time;
	}
}
