using System;

[Serializable]
public class BulletStatusEffectItemSynergy
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public float ChanceMultiplier = 1f;
}
