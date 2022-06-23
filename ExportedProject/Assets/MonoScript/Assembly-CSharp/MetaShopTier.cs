using System;

[Serializable]
public class MetaShopTier
{
	public int overrideTierCost = -1;

	[PickupIdentifier]
	public int itemId1 = -1;

	public int overrideItem1Cost = -1;

	[PickupIdentifier]
	public int itemId2 = -1;

	public int overrideItem2Cost = -1;

	[PickupIdentifier]
	public int itemId3 = -1;

	public int overrideItem3Cost = -1;
}
