using UnityEngine;

public class StickyFrictionModifier
{
	public float length;

	public float magnitude;

	public float elapsed;

	public bool usesFalloff = true;

	public StickyFrictionModifier(float l, float m, bool falloff = true)
	{
		length = l * GameManager.Options.StickyFrictionMultiplier;
		magnitude = Mathf.Clamp01(m);
		usesFalloff = falloff;
	}

	public float GetCurrentMagnitude()
	{
		if (usesFalloff)
		{
			float num = elapsed / length;
			float t = Mathf.Clamp01(num * num * num);
			return Mathf.Lerp(magnitude, 1f, t);
		}
		return magnitude;
	}
}
