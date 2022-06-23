using System;
using System.Collections.Generic;

[Serializable]
public class TertiaryBossRewardSet
{
	public string annotation = "reward";

	public float weight = 1f;

	[PickupIdentifier]
	public List<int> dropIds;
}
