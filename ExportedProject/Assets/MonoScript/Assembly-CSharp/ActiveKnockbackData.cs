using UnityEngine;

public class ActiveKnockbackData
{
	public Vector2 initialKnockback;

	public Vector2 knockback;

	public float elapsedTime;

	public float curveTime;

	public AnimationCurve curveFalloff;

	public GameObject sourceObject;

	public bool immutable;

	public ActiveKnockbackData(Vector2 k, float t, bool i)
	{
		knockback = k;
		initialKnockback = k;
		curveTime = t;
		immutable = i;
	}

	public ActiveKnockbackData(Vector2 k, AnimationCurve curve, float t, bool i)
	{
		knockback = k;
		initialKnockback = k;
		curveFalloff = curve;
		curveTime = t;
		immutable = i;
	}
}
