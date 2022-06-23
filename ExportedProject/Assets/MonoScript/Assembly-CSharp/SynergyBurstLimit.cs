using System;

[Serializable]
public struct SynergyBurstLimit
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public int limit;
}
