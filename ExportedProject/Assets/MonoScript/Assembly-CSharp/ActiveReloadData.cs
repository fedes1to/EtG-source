using System;

[Serializable]
public class ActiveReloadData
{
	public float damageMultiply = 1f;

	public float knockbackMultiply = 1f;

	public bool usesOverrideAngleVariance;

	public float overrideAngleVariance;

	public float reloadSpeedMultiplier = 1f;

	public bool ActiveReloadStacks;

	public bool ActiveReloadIncrementsTier;

	public int MaxTier = 5;
}
