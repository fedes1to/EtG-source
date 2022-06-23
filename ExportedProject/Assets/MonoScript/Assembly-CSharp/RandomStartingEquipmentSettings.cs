using System;
using System.Collections.Generic;

[Serializable]
public class RandomStartingEquipmentSettings
{
	public float D_CHANCE = 0.5f;

	public float C_CHANCE = 0.4f;

	public float B_CHANCE = 0.3f;

	public float A_CHANCE = 0.05f;

	public float S_CHANCE = 0.05f;

	[PickupIdentifier]
	public List<int> ExcludedPickups;
}
