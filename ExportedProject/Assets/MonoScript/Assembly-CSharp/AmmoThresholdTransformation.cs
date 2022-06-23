using System;

[Serializable]
public struct AmmoThresholdTransformation
{
	public float AmmoPercentage;

	[PickupIdentifier]
	public int TargetGunID;

	public bool HasSynergyChange;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public float SynergyAmmoPercentage;

	public float GetAmmoPercentage()
	{
		int count = -1;
		if (HasSynergyChange && PlayerController.AnyoneHasActiveBonusSynergy(RequiredSynergy, out count))
		{
			return SynergyAmmoPercentage;
		}
		return AmmoPercentage;
	}
}
