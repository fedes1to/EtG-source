using System;

[Serializable]
public struct NumericSynergyMultiplier
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public float SynergyMultiplier;
}
