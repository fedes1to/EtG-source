using System;

[Serializable]
public class LootModData
{
	[PickupIdentifier]
	public int AssociatedPickupId = -1;

	public float DropRateMultiplier;
}
